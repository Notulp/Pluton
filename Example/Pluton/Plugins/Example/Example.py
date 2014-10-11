import clr
import sys
from System import String

class Example:
	def On_PlayerConnected(self, player):
		for p in Server.ActivePlayers:
			if(p.Name != player.Name):
				p.Message(String.Format("{0} has joined the server!", player.Name))

	def On_PlayerDisconnected(self, player):
		for p in Server.ActivePlayers:
			if(p.Name != player.Name):
				p.Message(String.Format("{0} has left the server!", player.Name))
