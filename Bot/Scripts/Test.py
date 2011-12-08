import sys
import clr
clr.AddReference('Bot.Core')
clr.AddReference('Meebey.SmartIrc4net')
from Bot.Core.Commands import Command
from Meebey.SmartIrc4net import SendType

class IronPythonTest(Command):

	def Execute(self, e):
		e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, "Message from IronPython")

	def Name(self):
		return "ip"