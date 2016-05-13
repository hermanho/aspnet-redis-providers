//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microsoft.Web.Redis
{
    internal static class RedisUtility
    {
        private static Newtonsoft.Json.JsonSerializerSettings _jsonsetting = new Newtonsoft.Json.JsonSerializerSettings
        {
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
            TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
            ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        public static int AppendRemoveItemsInList(ChangeTrackingSessionStateItemCollection sessionItems, List<object> list)
        {
            int noOfItemsRemoved = 0;
            if (sessionItems.GetDeletedKeys() != null && sessionItems.GetDeletedKeys().Count != 0)
            {
                foreach (string delKey in sessionItems.GetDeletedKeys())
                {
                    list.Add(delKey);
                    noOfItemsRemoved++;
                }
            }
            return noOfItemsRemoved;
        }

        public static int AppendUpdatedOrNewItemsInList(ChangeTrackingSessionStateItemCollection sessionItems, List<object> list)
        {
            int noOfItemsUpdated = 0;
            if (sessionItems.GetModifiedKeys() != null && sessionItems.GetModifiedKeys().Count != 0)
            {
                foreach (string key in sessionItems.GetModifiedKeys())
                {
                    list.Add(key);
                    list.Add(GetBytesFromObject(sessionItems[key]));
                    noOfItemsUpdated++;
                }
            }
            return noOfItemsUpdated;
        }

        public static List<object> GetNewItemsAsList(ChangeTrackingSessionStateItemCollection sessionItems)
        {
            List<object> list = new List<object>();
            foreach (string key in sessionItems.Keys)
            {
                list.Add(key);
                list.Add(GetBytesFromObject(sessionItems[key]));
            }
            return list;
        }
        
        internal static byte[] GetBytesFromObject(object data)
        {
            if (data == null)
            {
                data = new RedisNull();
            }
            string s = Newtonsoft.Json.JsonConvert.SerializeObject(data, _jsonsetting);
            byte[] objectDataAsStream = System.Text.Encoding.Unicode.GetBytes(s);
            return objectDataAsStream;
        }

        internal static object GetObjectFromBytes(byte[] dataAsBytes)
        {
            if (dataAsBytes == null)
            {
                return null;
            }
            string s = System.Text.Encoding.Unicode.GetString(dataAsBytes);
            object retObject = Newtonsoft.Json.JsonConvert.DeserializeObject(s, _jsonsetting);
            return retObject;
        }
    }
}
