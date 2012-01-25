#
# Two simple commands in IronPython
#

import sys
import clr
clr.AddReference('Bot.Core')
clr.AddReference('Meebey.SmartIrc4net')
from Bot.Core.Commands import Command
from Meebey.SmartIrc4net import SendType


class PythonVersion(Command):

	def Execute(self, e):
		e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, "IronPython version: " + sys.version)

	def get_Name(self):
		return "pv"

	def get_Help(self):
		return "Displays the current IronPython version"


class Sing(Command):

	def Execute(self, e):
		e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, "Trodilididoo~~")

	def get_Name(self):
		return "sing"


class PythonEval(Command):

	def Execute(self, e):
		user = userService.GetAuthenticatedUser(e.Data.From)
		if user and user.UserLevel == 10:
			e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, str(eval(e.Data.Message.split(" ", 1)[-1], {'__builtins__':[]}, {})))
		else:
			e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "You do not have authorization to use this command")

	def get_Name(self):
		return "p"