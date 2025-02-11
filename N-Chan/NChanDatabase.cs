namespace GigaSharp;

using Microsoft.Data.Sqlite;

public class NChanDatabase
{
    public static void CreateDatabase(SqliteConnection conn){
        SqliteCommand com = conn.CreateCommand();
        com.CommandText = File.ReadAllText(Path.Combine(MasterProcess.sqlScriptsDirectory, "nchan-create-db.sql"));
        com.ExecuteNonQuery();
    }

    // ----------------------------- IMPORTANT -----------------------------
    // Remember to ALWAYS check the MasterProcess.databaseReady flag before any and all database operations
    // (except for creating it, of course). The GigaSharp database will be *massive* and we need to account for how long
    // it will take to download it from its remote host.

    public static Book GetBook(int id){
        if(!MasterProcess.databaseReady){return null;}
        Book.BookBuilder builder = new Book.BookBuilder();
        SqliteConnection conn = new SqliteConnection(MasterProcess.databaseConnectionString);
        conn.Open();
        SqliteCommand com = conn.CreateCommand();
        com.CommandText = File.ReadAllText(Path.Combine(MasterProcess.sqlScriptsDirectory, "nchan-get-book-full.sql"));
        com.Parameters.AddWithValue("$id", id);
        SqliteDataReader res = com.ExecuteReader();
        if(!res.Read()){
            res.Close();
            conn.Close();
            return null;
        }
        builder.AddId(id);
        builder.AddName(res.GetString(1));
        builder.AddPages(res.GetInt32(2));
        builder.AddFirstPage(res.GetString(3));
        int maxCapacity = res.GetInt32(0);
        HashSet<string> languages = new HashSet<string>(); //Allegedly, when C# constructs a default HashSet, its
        HashSet<string> tags = new HashSet<string>(maxCapacity); //initial capacity is 4. Once a HashSet needs to be
        HashSet<string> parodies = new HashSet<string>(); //resized, it's resized to double its current capacity. In
        HashSet<string> characters = new HashSet<string>(); //the overwhelming majority of cases, 4 is enough for all of
        HashSet<string> artists = new HashSet<string>(); //these lists apart from tags, and to avoid a lot of new sets
        HashSet<string> groups = new HashSet<string>(); //being created internally with continuously bigger and often
        HashSet<string> categories = new HashSet<string>(); //wasteful sizes, we'll adjust its initial capacity right now.
        //It should be noted that the above information about the default set size doesn't come from an official source,
        //in fact I couldn't find any microsoft documentation on HashSet's official sizes, rather just a blog post,
        //so it's not strictly trustworthy. It sounds plausible, though, so I'll believe it, since I couldn't find
        //anything to the contrary.
        do{
            //Getting a book from the database is the operation that inspired me to use HashSets for these lists, and
            //this loop is the main reason why. Given that all of these are stored on separate tables, our choices for
            //a "get book" database operation are perform multiple queries to get each table's contents and add them all,
            //which is an ok but verbose solution, or performing a single query and dealing with the fact that there will
            //be repeated information in different rows. HashSets allow us to do this latter approach without worrying
            //about checking for duplicates, given that they don't allow them by default in an O(1) operation.
            artists.Add(res.GetString(4));
            tags.Add(res.GetString(5));
            parodies.Add(res.GetString(6));
            characters.Add(res.GetString(7));
            groups.Add(res.GetString(8));
            categories.Add(res.GetString(9));
            languages.Add(res.GetString(10));
        }while(res.Read());
        res.Close();
        conn.Close();
        builder.AddLanguages(languages);
        builder.AddTags(tags);
        builder.AddParody(parodies);
        builder.AddCharacters(characters);
        builder.AddArtists(artists);
        builder.AddGroups(groups);
        builder.AddCategories(categories);
        return builder.Build();
    }

    public static bool InsertBook(Book book){
        if(!MasterProcess.databaseReady){return false;}
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
        InsertBookAncillaryInfo(book.Id, book.Artists, "artist", "artists", com);
        InsertBookAncillaryInfo(book.Id, book.Tags, "tag", "tags", com);
        InsertBookAncillaryInfo(book.Id, book.Parody, "parody", "parodies", com);
        InsertBookAncillaryInfo(book.Id, book.Characters, "character", "characters", com);
        InsertBookAncillaryInfo(book.Id, book.Groups, "group", "groups", com);
        InsertBookAncillaryInfo(book.Id, book.Categories, "category", "categories", com);
        InsertBookAncillaryInfo(book.Id, book.Languages, "language", "languages", com);
        conn.Close();
        return true;
    }

    private static void InsertBookAncillaryInfo(int id, HashSet<string> info, string infoNameSingular, string infoNamePlural, SqliteCommand com){
        
        //This method is quite confusing, and worse than that, extremely important, so I'll provide a
        //fairly detailed explanation of its workings.

        //First, we iterate over every item in the list, obviously.
        foreach(string item in info){
            //We will first try to search the list's table for each item's ID.
            com.CommandText = "SELECT id FROM " + infoNameSingular + " WHERE name = $name";
            com.Parameters.AddWithValue("$name", item); //Titles can have special characters, so this way of adding parameters is important.
            SqliteDataReader res = com.ExecuteReader();
            com.Parameters.Clear(); // Clearing may not be strictly necessary given that we'll reuse the parameter, but just to be safe.
            if(!res.Read()){ // Should the query not return anything, we'll instead attempt to insert the item
                res.Close();
                com.CommandText = "INSERT INTO "+infoNameSingular+" (name) VALUES ($name)";
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
                com.CommandText = "SELECT id FROM " + infoNameSingular + " WHERE name = $name";
                com.Parameters.AddWithValue("$name", item);
                res = com.ExecuteReader();
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