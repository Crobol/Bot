#
# Three simple commands in IronPython
#

import sys
	
def PrintVersion():
	return ("Python version: " + sys.version)

def PrintParam(param):
	return ("Param: " + param)

def main(argv):
	print "Test" + argv[0]

if __name__ == "__main__":
	main(sys.argv[1:])