import sys
import clr
clr.AddReference('Bot.Core')
clr.AddReference('Meebey.SmartIrc4net')
from Bot.Core.Commands import Command
from Meebey.SmartIrc4net import SendType

#class PythonEval(Command):
#
#	def Execute(self, e):
#		e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, str(eval(e.Data.Message.split(" ", 1)[-1], {'__builtins__':[]}, {})))
#
#	def Name(self):
#		return "p"


class PythonVersion(Command):

	def Execute(self, e):
		e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, "IronPython version: " + sys.version)

	def Name(self):
		return "pv"

	def Help(self):
		return "Displays the current IronPython version"


class Sing(Command):

	def Execute(self, e):
		e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, "Trodilididoo~~")

	def Name(self):
		return "sing"
