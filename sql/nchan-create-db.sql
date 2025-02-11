CREATE TABLE IF NOT EXISTS artist ( id INTEGER PRIMARY KEY, name VARCHAR(255) );
CREATE TABLE IF NOT EXISTS tag ( id INTEGER PRIMARY KEY, name VARCHAR(255) );
CREATE TABLE IF NOT EXISTS parody ( id INTEGER PRIMARY KEY, name VARCHAR(255) );
CREATE TABLE IF NOT EXISTS character ( id INTEGER PRIMARY KEY, name VARCHAR(255) );
CREATE TABLE IF NOT EXISTS group ( id INTEGER PRIMARY KEY, name VARCHAR(255) );
CREATE TABLE IF NOT EXISTS category ( id INTEGER PRIMARY KEY, name VARCHAR(255) );
CREATE TABLE IF NOT EXISTS language ( id INTEGER PRIMARY KEY, name VARCHAR(255) );

CREATE TABLE IF NOT EXISTS book (
    id INTEGER PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    firstPage VARCHAR(255),
    pages INTEGER DEFAULT 0,
) WITHOUT ROWID;

CREATE TABLE IF NOT EXISTS book_artists (
    book_id INTEGER,
    artist_id INTEGER,
    PRIMARY KEY (book_id, artist_id),
    FOREIGN KEY (book_id) REFERENCES book(id),
    FOREIGN KEY (artist_id) REFERENCES artist(id)
);

CREATE TABLE IF NOT EXISTS book_tags (
    book_id INTEGER,
    tag_id INTEGER,
    PRIMARY KEY (book_id, tag_id),
    FOREIGN KEY (book_id) REFERENCES book(id),
    FOREIGN KEY (tag_id) REFERENCES tag(id)
);

CREATE TABLE IF NOT EXISTS book_parodies (
    book_id INTEGER,
    parody_id INTEGER,
    PRIMARY KEY (book_id, parody_id),
    FOREIGN KEY (book_id) REFERENCES book(id),
    FOREIGN KEY (parody_id) REFERENCES parody(id)
);

CREATE TABLE IF NOT EXISTS book_groups (
    book_id INTEGER,
    group_id INTEGER,
    PRIMARY KEY (book_id, group_id),
    FOREIGN KEY (book_id) REFERENCES book(id),
    FOREIGN KEY (group_id) REFERENCES group(id)
);

CREATE TABLE IF NOT EXISTS book_characters (
    book_id INTEGER,
    character_id INTEGER,
    PRIMARY KEY (book_id, character_id),
    FOREIGN KEY (book_id) REFERENCES book(id),
    FOREIGN KEY (character_id) REFERENCES character(id)
);

CREATE TABLE IF NOT EXISTS book_categories (
    book_id INTEGER,
    category_id INTEGER,
    PRIMARY KEY (book_id, category_id),
    FOREIGN KEY (book_id) REFERENCES book(id),
    FOREIGN KEY (category_id) REFERENCES category(id)
);

CREATE TABLE IF NOT EXISTS book_languages (
    book_id INTEGER,
    language_id INTEGER,
    PRIMARY KEY (book_id, language_id),
    FOREIGN KEY (book_id) REFERENCES book(id),
    FOREIGN KEY (language_id) REFERENCES language(id)
);

CREATE TABLE IF NOT EXISTS book_of_the_day (
    id INTEGER PRIMARY KEY,
    day DATE NOT NULL,
    book_id INTEGER,
    FOREIGN KEY (book_id) REFERENCES book(id),
);