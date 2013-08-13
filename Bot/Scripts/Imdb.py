#!/usr/bin/env python
#
# Bot: alias=imdb,IMDb
#

import sys
import imdb

def imdb(argv):
	imdb_access = imdb.IMDb()

	movie_args = ""
	for arg in argv:
		movie_args += arg + " "
	search_result = imdb_access.search_movie(movie_args)
	movie = search_result[0]
	imdb_access.update(movie)
	print_string = movie['long imdb canonical title'] + " " + str(movie['rating']) + u" \u2605"
	genre_string = "  " + movie['genre'][0]
	for genre in movie['genre'][1:]:
		genre_string += " | " + genre
	print_string += genre_string
	return print_string

if __name__ == "__main__":
	main(sys.argv[1:])