using Simon.SharpStreaming.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
	class Program
	{

		public static string MD5(string input)
		{
			// Use input string to calculate MD5 hash
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
				byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
				byte[] hashBytes = md5.ComputeHash(inputBytes);

				// Convert the byte array to hexadecimal string
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++) {
					sb.Append(hashBytes[i].ToString("x2"));
				}
				return sb.ToString();
			}
		}

		static Dictionary<string, string> ParseDigest(string digest)
		{
			string DIGEST = "Digest";
			Dictionary<string, string> dict = new Dictionary<string, string>();
			int k = digest.IndexOf(DIGEST);
			if (k > 0) {
				string content = digest.Substring(k + DIGEST.Length);
				if (!string.IsNullOrEmpty(content)) content = content.Trim();
				string[] lines = content.Split(new char[] { ',' });
				foreach (string l in lines) {
					var m = l.Trim();
					var p = m.Split(new char[] { '=' });
					var key = p[0];
					if (!dict.ContainsKey(key)) {
						var val = p[1].Trim();
						if (val.StartsWith("\"") && val.EndsWith("\"")) {
							val = val.Substring(1, val.Length - 2);
						}
						dict.Add(key, val);
					}
				}
			}
			return dict;
		}

		static void Main(string[] args)
		{
			int seq = 0;
			string url = "rtsp://144.92.136.102";

			RtspClient rtspClient = new RtspClient();
			bool result = rtspClient.ConnectServer("144.92.136.102", 554);

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0} ", Constants.RTSP_CMD_OPTIONS);
			sb.AppendFormat("{0} RTSP/1.0\r\n", url);
			sb.AppendFormat("CSeq: {0}\r\n", (++seq).ToString());
			sb.AppendFormat("User-Agent: {0}\r\n\r\n", Constants.USER_AGENT_HEADER);

			rtspClient.SendMessage(sb.ToString());
			string response = rtspClient.ReceiveMessage();
			Console.WriteLine(response);

			string[] lines = response.Split(new char[] { '\r', '\n' });
			string re="";
			Dictionary<string, string> p = null;
			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].StartsWith("WWW-Authenticate")) {
					Console.WriteLine(lines[i]);
					p = ParseDigest(lines[i]);
					//HA1 = MD5(username: realm:password)
					//HA2 = MD5(method: digestURI)
					//response = MD5(HA1: nonce:HA2)
					var HA1 = MD5("admin:" + p["realm"] +":1675WisM@d");
					var HA2 = MD5(Constants.RTSP_CMD_OPTIONS +":" + url  );
					re = MD5(HA1 + ":" + p["nonce"] + ":" + HA2);
				}
			}


			sb = new StringBuilder();
			sb.AppendFormat("{0} ", Constants.RTSP_CMD_OPTIONS);
			sb.AppendFormat("{0} RTSP/1.0\r\n", url);
			sb.AppendFormat("CSeq: {0}\r\n", (++seq).ToString());
			sb.AppendFormat("Authorization: Digest username=\"{0}\", realm=\"{1}\", nonce=\"{2}\", uri=\"{3}\", response=\"{4}\"\r\n", "admin", p["realm"], p["nonce"], url, re);
			sb.AppendFormat("User-Agent: {0}\r\n\r\n", Constants.USER_AGENT_HEADER);

			rtspClient.SendMessage(sb.ToString());  
			response = rtspClient.ReceiveMessage();

			Console.WriteLine(response);
		}
	}
}
