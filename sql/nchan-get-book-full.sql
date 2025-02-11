SELECT COUNT(*), book.title, book.pages, book.firstPage, artist.name AS artist_name, tag.name AS tag_name,
parody.name AS parody_name, character.name AS character_name, group.name AS group_name, category.name AS category_name,
language.name AS language_name
FROM book
INNER JOIN book_artists ON book_artists.id = book.id
INNER JOIN artist ON artist.id = book_artists.id
INNER JOIN book_tags ON book_tags.id = book.id
INNER JOIN tag ON tag.id = book_tags.id
INNER JOIN book_parodies ON book_parodies.id = book.id
INNER JOIN parody ON parody.id = book_parodies.id
INNER JOIN book_characters ON book_characters.id = book.id
INNER JOIN character ON character.id = book_characters.id
INNER JOIN book_groups ON book_groups.id = book.id
INNER JOIN group ON group.id = book_groups.id
INNER JOIN book_categories ON book_categories.id = book.id
INNER JOIN category ON category.id = book_categories.id
INNER JOIN book_languages ON book_languages.id = book.id
INNER JOIN language ON language.id = book_languages.id
WHERE book.id = $id