using System;

namespace ServerClientLib
{
    public enum TargetMsg { SERVER, ALL, ALLES }
    public enum HeaderRequest { CREATE_ROOM, JOIN_ROOM, MATCHMAKING, CHANGE_NAME, START_TCP_HOLE_PUNCHING }

    public static class ServerClient
    {
        public static string TakeMessageInBack(string message)
        {
            int idx     = message.IndexOf("|", 0) + 1;
            string msg  = message.Substring(idx, message.Length - idx);
            return msg;
        }

    }
}
