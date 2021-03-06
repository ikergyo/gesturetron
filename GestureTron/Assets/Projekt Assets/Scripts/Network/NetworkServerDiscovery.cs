using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkServerDiscovery : NetworkDiscovery
{
    public GameListController glc;

    private float timeout = 5f;

    private Dictionary<LanConnectionInfo, float> lanAddresses = new Dictionary<LanConnectionInfo, float>();

    private void Awake()
    {
        base.Initialize();
        base.StartAsClient();
    }


    public void StartBroadcast()
    {
        StopBroadcast();
        base.Initialize();
        base.StartAsServer();
    }

    private IEnumerator CleanupExpiredGames()
    {
        while (true)
        {
            bool changed = false;
            var keys = lanAddresses.Keys.ToList();
            foreach (var key in keys)
            {
                if (lanAddresses[key] <= Time.time)
                {
                    lanAddresses.Remove(key);
                    changed = true;
                }

                if (changed)
                {
                    //UpdateMatchInfos();
                }
                yield return new WaitForSeconds(timeout);
            }
        }
    }
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        base.OnReceivedBroadcast(fromAddress, data);
         LanConnectionInfo info = new LanConnectionInfo("local", fromAddress, data);
        
         if (lanAddresses.ContainsKey(info) == false)
         {
             lanAddresses.Add(info, Time.time);
             glc.AddServer(info);
        }
         else
         {
             lanAddresses[info] = Time.time + timeout;
         }
    }


}
