﻿/*
Abbot: The petite IRC bot
Copyright (C) 2005 Hannes Sachsenhofer

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

#region Using directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
#endregion

namespace Abbot.Plugins {
	public class Quote : Plugin {

		#region " Constructor/Destructor "
		List<QuoteInfo> quotes;
		Random r = new Random();
		public Quote(Abbot bot):base(bot) {
			Bot.Message += new MessageEventHandler(Bot_Message);
			Bot.Disconnect += new DisconnectEventHandler(Bot_Disconnect);
			Load();
		}
		#endregion

		#region " Quote "
		string GetQuote() {
			return GetQuote(quotes);
		}

		string GetQuote(string type) {
			List<QuoteInfo> l = new List<QuoteInfo>();
			foreach (QuoteInfo q in quotes)
				if (q.Type.ToLower() == type.ToLower())
					l.Add(q);
			return GetQuote(l);
		}

		string GetQuote(List<QuoteInfo> l) {
			if (l.Count <= 0)
				return "";
			return l[r.Next(0, l.Count)].Text;
		}

		#region " Load/Save (Serialization) "
		public void Save() {
			StreamWriter f = new StreamWriter("Data\\Quotes.xml", false);
			new XmlSerializer(typeof(List<QuoteInfo>)).Serialize(f, quotes);
			f.Close();
		}

		public void Load() {
			try {
				FileStream f = new FileStream("Data\\Quotes.xml", FileMode.Open);
				quotes = (List<QuoteInfo>)new XmlSerializer(typeof(List<QuoteInfo>)).Deserialize(f);
				f.Close();
			} catch (Exception e) {
				Console.WriteLine("# " + e.Message);
				quotes = new List<QuoteInfo>();
			}
		}
		#endregion

		#region " QuoteInfo "
		public class QuoteInfo {

			public QuoteInfo() { }

			public QuoteInfo(string type, string text) {
				this.type = type;
				this.text = text;
			}

			string type;
			public string Type {
				get {
					return type;
				}
				set {
					type = value;
				}
			}

			string text;
			public string Text {
				get {
					return text;
				}
				set {
					text = value;
				}
			}
		}
		#endregion
		#endregion

		#region " Event handles "
		void Bot_Disconnect(string network) {
			Bot.Write(network, "QUIT :" + GetQuote());
		}

		void Bot_Message(string network, string channel, string user, string message) {
			Regex r;

			r = new Regex(@"^quote$");
			if (r.IsMatch(message)) {
				string s = GetQuote();
				if (s.Length <= 0) {
					Bot.WriteNotice(network, Helper.GetNickFromUser(user), "I'm sorry, I don't know any quotes.");
					return;
				}
				Bot.Write(network, channel, s);
				return;
			}

			r = new Regex(@"^quote (?<type>\w*)$");
			if (r.IsMatch(message)) {
				Match m = r.Match(message);
				string s = GetQuote(m.Groups["type"].Value);
				if (s.Length <= 0) {
					Bot.WriteNotice(network, Helper.GetNickFromUser(user), "I'm sorry, I don't know any '" + m.Groups["type"].Value + "'quotes.");
					return;
				}
				Bot.Write(network, channel, s);
				return;
			}

			r = new Regex(@"^add (?<type>\w*?) quote (?<text>.*)$");
			if (r.IsMatch(message)) {
				Match m = r.Match(message);
				quotes.Add(new QuoteInfo(m.Groups["type"].Value, m.Groups["text"].Value));
				Bot.WriteNotice(network, Helper.GetNickFromUser(user), "Your quote has been added.");
				Save();
				return;
			}
		}
		#endregion
	}
}
