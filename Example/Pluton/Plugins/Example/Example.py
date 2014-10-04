import clr
import sys
clr.AddReferenceByPartialName("UnityEngine")
clr.AddReferenceByPartialName("Pluton")
import UnityEngine
import Pluton
from Pluton import InvItem
from System import *
from UnityEngine import *

class Example:
	def On_PlayerConnected(self, player):
		for p in Server.ActivePlayers:
			if(p.Name != player.Name):
				p.Message(String.Format("{0} has joined the server!", player.Name))

	def On_PlayerDisconnected(self, player):
		for p in Server.ActivePlayers:
			if(p.Name != player.Name):
				p.Message(String.Format("{0} has left the server!", player.Name))

	def On_Command(self, cmd):
		try:
			if(cmd.cmd == "kit"):
				if(Server.LoadOuts.ContainsKey(cmd.quotedArgs[0]))
				loadout = Server.LoadOuts[cmd.quotedArgs[0]]
				loadout.ToInv(cmd.User.Inventory)
			if(cmd.cmd == "apple"):
				cmd.User.Message("An apple a day keeps the doctor away!")
				item = InvItem("Apple")
				item.Instantiate(Vector3(cmd.User.X + 3, cmd.User.Y + 3, cmd.User.Z + 3))
			if(cmd.cmd == "help"):
				cmd.User.Message("Usable command: /whereami, /kit starter")
		except:
			Debug.Log(String.Format("Something went wrong while executing: /{0} args", cmd.cmd, String.Join(" ", cmd.args)))