namespace GigaSharp;

using Microsoft.Data.Sqlite;

public class NChanDatabase
{
    //This flag marks whether or not a modification is currently underway in N-Chan's portion of the
    //database. This is necessary because we do not want two books to be able to be written at the same
    //time, because if they're the same book, we'll cause a database error.
    public static bool modificationOngoing = false;
    public static void CreateDatabase(SqliteConnection conn){
        SqliteCommand com = conn.CreateCommand();
        com.CommandText = File.ReadAllText(Path.Combine(MasterProcess.sqlScriptsDirectory, "nchan-create-db.sql"));
        com.ExecuteNonQuery();
    }

    // ----------------------------- IMPORTANT -----------------------------
    // Remember to ALWAYS check the MasterProcess.databaseReady flag before any and all database operations
    // (except for creating it, of course). The GigaSharp database will be *massive* and we need to account for how long
    // it will take to download it from its remote host.
    // Also remember to check for and delay/cancel any write operations should the modificationOngoing flag be set.

    public static Book GetBook(int id){
        if(!MasterProcess.databaseReady){return null;}
        Book.BookBuilder builder = new Book.BookBuilder();
        SqliteConnection conn = new SqliteConnection(MasterProcess.databaseConnectionString);
        conn.Open();
        SqliteCommand com = conn.CreateCommand();
        com.CommandText = "SELECT title, pages, firstPage FROM book WHERE id = $id";
        com.Parameters.AddWithValue("$id", id);
        SqliteDataReader res = com.ExecuteReader();
        com.Parameters.Clear();
        if(!res.Read()){
            res.Close();
            conn.Close();
            return null;
        }
        builder.AddId(id);
        builder.AddName(res.GetString(0));
        builder.AddPages(res.GetInt32(1));
        builder.AddFirstPage(res.GetString(2));
        res.Close();
        //What you see below is a generic function being used to fill the hash sets of the book.
        //My original idea was to just have one big query that would return everything, but I eventually
        //ran into problems when it comes to books that don't have all their ancillary info (like groups)
        //Given that we have an auxiliary table connecting the book table and each list's table, if I
        //tried using INNER JOIN for everything, there not being an overlap between the book and a list
        //made it so nothing got returned. I also tried using LEFT/RIGHT join, but they wouldn't work for
        //the connection between the auxiliary table and the list table, and INNER there caused the same
        //problem. So, we just use several queries instead.
        //It's probably possible to just create 1 query, but I'm not a database engineer.
        builder.AddLanguages(GetBookAncillaryInfo(id, "language", "languages", com));
        builder.AddTags(GetBookAncillaryInfo(id, "tag", "tags", com));
        builder.AddParody(GetBookAncillaryInfo(id, "parody", "parodies", com));
        builder.AddCharacters(GetBookAncillaryInfo(id, "character", "characters", com));
        builder.AddArtists(GetBookAncillaryInfo(id, "artist", "artists", com));
        builder.AddGroups(GetBookAncillaryInfo(id, "group", "groups", com));
        builder.AddCategories(GetBookAncillaryInfo(id, "category", "categories", com));
        conn.Close();
        return builder.Build();
    }

    private static HashSet<string> GetBookAncillaryInfo(int id, string infoNameSingular, string infoNamePlural, SqliteCommand com){
        //QUICK EXPLANATION: You might be wondering why there's a COUNT(*) here. Well, it's to save memory.
        //According to a tech blog I saw (Microsoft's documentation doesn't provide any answers to this),
        //when a C# HashSet is constructed without capacity information, its default capacity is 4 items.
        //And when hashsets resize they create a new data structure that has double the capacity. This is
        //of course to be avoided if possible, especially because it creates many garbage structures that
        //aren't used and get GC'd, and given that there can be *many* tags, we have COUNT(*) there for
        //the small memory optimization.
        com.CommandText = "SELECT \""+infoNameSingular+"\".name AS name FROM book"
            +" INNER JOIN book_"+infoNamePlural+" ON book.id = book_"+infoNamePlural+".book_id"
            +" INNER JOIN \""+infoNameSingular+"\" ON \""+infoNameSingular+"\".id = book_"+infoNamePlural+"."+infoNameSingular+"_id"
            +" WHERE book.id = $id";
        com.Parameters.AddWithValue("$id", id);
        SqliteDataReader res = com.ExecuteReader();
        com.Parameters.Clear();
        if(!res.Read()){
            res.Close();
            return null;
        }
        HashSet<string> set = new HashSet<string>();
        do{
            set.Add(res.GetString(0));
        }while(res.Read());
        res.Close();
        return set;
    }

    public static bool InsertBook(Book book){
        if(!MasterProcess.databaseReady || modificationOngoing){return false;}
        modificationOngoing = true;
        SqliteConnection conn = new SqliteConnection(MasterProcess.databaseConnectionString);
        conn.Open();
        SqliteCommand com = conn.CreateCommand();
        com.CommandText = @"INSERT INTO book (id, title, firstPage, pages)
            VALUES ($id, $title, $firstpage, $pages)";
        com.Parameters.AddWithValue("$id", book.Id);
        com.Parameters.AddWithValue("$title", book.Name);
        com.Parameters.AddWithValue("$firstpage", book.FirstPage);
        com.Parameters.AddWithValue("$pages", book.Pages);
        if(com.ExecuteNonQuery() != 1){
            Console.WriteLine("BOOK INSERTION FAILED - COMMAND TEXT WAS:\n"+com.CommandText);
            return false;
        }
        com.Parameters.Clear();
        if(book.Artists != null && book.Artists.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Artists, "artist", "artists", com); }
        if(book.Tags != null && book.Tags.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Tags, "tag", "tags", com); }
        if(book.Parody != null && book.Parody.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Parody, "parody", "parodies", com); }
        if(book.Characters != null && book.Characters.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Characters, "character", "characters", com); }
        if(book.Groups != null && book.Groups.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Groups, "group", "groups", com); }
        if(book.Categories != null && book.Categories.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Categories, "category", "categories", com); }
        if(book.Languages != null && book.Languages.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Languages, "language", "languages", com); }
        conn.Close();
        modificationOngoing = false;
        return true;
    }

    private static void InsertBookAncillaryInfo(int id, HashSet<string> info, string infoNameSingular, string infoNamePlural, SqliteCommand com){
        
        //This method is quite confusing, and worse than that, extremely important, so I'll provide a
        //fairly detailed explanation of its workings.

        //First, we iterate over every item in the list, obviously.
        foreach(string item in info){
            //We will first try to search the list's table for each item's ID.
            com.CommandText = "SELECT id FROM \"" + infoNameSingular + "\" WHERE name = $name";
            com.Parameters.AddWithValue("$name", item); //Titles can have special characters, so this way of adding parameters is important.
            SqliteDataReader res = com.ExecuteReader();
            com.Parameters.Clear(); // Clearing may not be strictly necessary given that we'll reuse the parameter, but just to be safe.
            if(!res.Read()){ // Should the query not return anything, we'll instead attempt to insert the item
                res.Close();
                com.CommandText = "INSERT INTO \""+infoNameSingular+"\" (name) VALUES ($name)";
                com.Parameters.AddWithValue("$name", item);
                if(com.ExecuteNonQuery() != 1){ //ExecuteNonQuery tell us the nº of affected rows: If it's not 1, something has gone wrong.
                    com.Parameters.Clear();
                    Console.WriteLine("BOOK INFO INSERTION FAILED - COMMAND TEXT WAS:\n"+com.CommandText);
                    continue;
                    //It's at this point that I feel obliged to point out that this is actually quite terrible.
                    //What SHOULD be happening here is that this failure throws an exception and causes the entire insertion
                    //process to be rolled back (which *is* possible in SQLite): But I've elected, against my best senses,
                    //to not do that.
                    //This is because transactions in SQLite 1- Are strictly blocking procedures, meaning two transactions
                    //can never occur at the same time. Microsoft's documentation warns that this might cause transactions
                    //to time out often, and this is even more so the case because discord commands time out very quick.
                    //True enough that this doesn't matter too much given the bot will barely see two people using it at once,
                    //but more importantly, 2- Are a pain in the ass to implement, and I don't want to do that.
                }
                com.Parameters.Clear(); //With the insertion done, we will redo the intial command.
                com.CommandText = "SELECT id FROM \"" + infoNameSingular + "\" WHERE name = $name";
                com.Parameters.AddWithValue("$name", item);
                res = com.ExecuteReader();
                res.Read();
                com.Parameters.Clear();
                //Yes, I know, the constant clearing seems redundant, and it probably is, but SQLite is finnicky as hell,
                //and I don't want to take any chances.
            }
            //By this point, the res reader has the item's ID for sure.
            int itemId = res.GetInt32(0);
            res.Close(); //And now we simply insert the required row into the auxiliary table.
            com.CommandText = "INSERT INTO book_"+infoNamePlural+" (book_id, "+infoNameSingular+"_id) VALUES ("+id+", "+itemId+")";
            if(com.ExecuteNonQuery() != 1){
                Console.WriteLine("BOOK INFO INSERTION FAILED - COMMAND TEXT WAS:\n"+com.CommandText);
                //Once again, this suffers from the exact same problem I described above with the lack of transactions.
                //No, I am not doing it unless it proves to be actually necessary.
            }
        }
    }
}