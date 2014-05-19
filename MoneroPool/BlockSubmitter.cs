﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MoneroPool
{
    public class BlockSubmitter
    {
        public BlockSubmitter(){}

        public async void Start()
        {
            Logger.Log(Logger.LogLevel.General, "Beginning Block Submittion thread!");

            await Task.Yield();
            while (true)
            {
                Thread.Sleep(5000);
                for (int i = 0; i < Statics.BlocksPendingSubmition.Count; i++)
                {
                    try
                    {
                        PoolBlock block = Statics.BlocksPendingSubmition[i];

                        JObject submitblock =
                            (await
                             Statics.DaemonJson.InvokeMethodAsync("submitblock",
                                                                  new JArray(
                                                                      BitConverter.ToString(block.BlockData)
                                                                                  .Replace("-", ""))));
                        if ((string) submitblock["result"]["status"] == "OK")
                        {
                            Block rBlock = Statics.RedisDb.Blocks.First(x => x.BlockHeight == block.BlockHeight); //
                            rBlock.Found = true;
                            rBlock.Founder = block.Founder;

                            Statics.RedisDb.SaveChanges(rBlock);

                            Statics.BlocksPendingPayment.Add(block);
                        }
                        else
                        {
                            Logger.Log(Logger.LogLevel.Error, "Block submittance failed with height {0} and error {1}!",
                                       block.BlockHeight, submitblock["result"]["status"]);
                        }
                        Statics.BlocksPendingSubmition.RemoveAt(i);
                        i--;
                    }
                    catch (Exception e)
                    {
                        Logger.Log(Logger.LogLevel.Error,e.ToString());
                    }
                }
            }
        }

    }
}
