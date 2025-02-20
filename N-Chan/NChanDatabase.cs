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

    public async static Task<bool> InsertBook(Book book){
        int secsElapsed = 0;
        while((!MasterProcess.databaseReady || MasterProcess.modificationOngoing) && secsElapsed < 5){
            await Task.Delay(1000);
            secsElapsed++;
        }
        if(secsElapsed == 5){return false;}
        MasterProcess.modificationOngoing = true;
        SqliteConnection conn = new SqliteConnection(MasterProcess.databaseConnectionString);
        await conn.OpenAsync();
        System.Data.Common.DbTransaction transaction = await conn.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try{
            SqliteCommand com = conn.CreateCommand();
            com.CommandText = @"INSERT INTO book (id, title, firstPage, pages)
                VALUES ($id, $title, $firstpage, $pages)";
            com.Parameters.AddWithValue("$id", book.Id);
            com.Parameters.AddWithValue("$title", book.Name);
            com.Parameters.AddWithValue("$firstpage", book.FirstPage);
            com.Parameters.AddWithValue("$pages", book.Pages);
            if(await com.ExecuteNonQueryAsync() != 1){
                throw new Exception("BOOK INFO INSERTION FAILED - COMMAND TEXT WAS:\n"+com.CommandText);
            }
            com.Parameters.Clear();
            if(book.Artists != null && book.Artists.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Artists, "artist", "artists", com); }
            if(book.Tags != null && book.Tags.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Tags, "tag", "tags", com); }
            if(book.Parody != null && book.Parody.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Parody, "parody", "parodies", com); }
            if(book.Characters != null && book.Characters.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Characters, "character", "characters", com); }
            if(book.Groups != null && book.Groups.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Groups, "group", "groups", com); }
            if(book.Categories != null && book.Categories.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Categories, "category", "categories", com); }
            if(book.Languages != null && book.Languages.Count > 0) { InsertBookAncillaryInfo(book.Id, book.Languages, "language", "languages", com); }
            await transaction.CommitAsync();
            await conn.CloseAsync();
            MasterProcess.modificationOngoing = false;
            return true;
        }catch (Exception e){
            Console.WriteLine(e.Message);
            await transaction.RollbackAsync();
            await conn.CloseAsync();
            return false;
        }
    }

    private async static void InsertBookAncillaryInfo(int id, HashSet<string> info, string infoNameSingular, string infoNamePlural, SqliteCommand com){
        
        //This method is quite confusing, and worse than that, extremely important, so I'll provide a
        //fairly detailed explanation of its workings.

        //First, we iterate over every item in the list, obviously.
        foreach(string item in info){
            //We will first try to search the list's table for each item's ID.
            com.CommandText = "SELECT id FROM \"" + infoNameSingular + "\" WHERE name = $name";
            com.Parameters.AddWithValue("$name", item); //Titles can have special characters, so this way of adding parameters is important.
            SqliteDataReader res = await com.ExecuteReaderAsync();
            com.Parameters.Clear(); // Clearing may not be strictly necessary given that we'll reuse the parameter, but just to be safe.
            if(!await res.ReadAsync()){ // Should the query not return anything, we'll instead attempt to insert the item
                res.Close();
                com.CommandText = "INSERT INTO \""+infoNameSingular+"\" (name) VALUES ($name)";
                com.Parameters.AddWithValue("$name", item);
                if(await com.ExecuteNonQueryAsync() != 1){ //ExecuteNonQuery tell us the nº of affected rows: If it's not 1, something has gone wrong.
                    com.Parameters.Clear();
                    throw new Exception("BOOK INFO INSERTION FAILED - COMMAND TEXT WAS:\n"+com.CommandText);
                }
                com.Parameters.Clear(); //With the insertion done, we will redo the intial command.
                com.CommandText = "SELECT id FROM \"" + infoNameSingular + "\" WHERE name = $name";
                com.Parameters.AddWithValue("$name", item);
                res = await com.ExecuteReaderAsync();
                res.Read();
                com.Parameters.Clear();
                //Yes, I know, the constant clearing seems redundant, and it probably is, but SQLite is finnicky as hell,
                //and I don't want to take any chances.
            }
            //By this point, the res reader has the item's ID for sure.
            int itemId = res.GetInt32(0);
            res.Close(); //And now we simply insert the required row into the auxiliary table.
            com.CommandText = "INSERT INTO book_"+infoNamePlural+" (book_id, "+infoNameSingular+"_id) VALUES ("+id+", "+itemId+")";
            if(await com.ExecuteNonQueryAsync() != 1){
                throw new Exception("BOOK INFO INSERTION FAILED - COMMAND TEXT WAS:\n"+com.CommandText);
            }
        }
    }
}