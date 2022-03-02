using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// RID/UID#content<EOF>

public static class PacketTools
{
    public static string PacketToUID(string packet){
       
        try{
            return packet.Split('#')[0].Split('/')[1];
        }
        catch{
            Debug.LogError(packet + " - couldn't get UID");
            return "1";
        }
    }

    // Check if packet requires response
    public static int RequireResponse(string packet){
        return int.Parse(packet.Split('#')[0].Split('/')[0]);
    }

    public static string Build(string packet, int require_response){
        packet = require_response.ToString() + "/" + packet;
        return packet + "<EOF>";
    }

    // Remove EOF
    public static string DeBuild(string packet){
        return packet.Substring(0, packet.IndexOf("<EOF>"));
    }

    public static byte[] Encode(string to_encode){
        return Encoding.UTF8.GetBytes(to_encode);
    }
}