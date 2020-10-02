using System;
namespace Atomic.Arc
{
    public static class AlarmDecoder
    {
        // Line number
        // Account number
        // Code
        // Zone
        // Area
        // User
        // Timestamp
        // New/Old
        // ASCII
        public static SiaAlarm Process(string data)
        {
            var alarm = new SiaAlarm
            {
                Interface = GetInterface(data.Substring(0, 4)) //Line number
            };

            AlarmFormat(alarm, data);

            return alarm;
        }

        private static string GetInterface(string lineNumber)
            => lineNumber switch
            {
                "S551" => "LAN",
                "S552" => "Radio 1",
                "S553" => "PSTN",
                "S554" => "System",
                "S555" => "Radio 2",
                _ => "Unknown",
            };

        private static void AlarmFormat(in SiaAlarm output, string input)
        {
            if (input.IndexOf("[") > -1) // SIA Alarm
            {
                DecodeSia(output, input);
                output.Decoded = true;
            }
            else
            {
                Console.WriteLine("Unsupported alarm format");
                output.Decoded = false;
            }
        }

        private static void DecodeSia(in SiaAlarm output, string input)
        {
            string sia = input.Substring(input.IndexOf("[") + 1, input.IndexOf("]") - 5);
            Console.WriteLine("SIA Alarm found: {0}", sia);
            string[] siaArray = sia.Split("|");

            //Account number
            output.AccountNumber = siaArray[0].Substring(1);
            //New/Old
            output.New = siaArray[1].Substring(0, 1) == "N";

            //Decode the SIA data section
            GetSiaData(output, siaArray[1].Substring(1));

            //ASCII
            if (siaArray.Length == 3)
            {
                output.Ascii = siaArray[2].Substring(1);
            }
        }

        private static void GetSiaData(in SiaAlarm output, string input)
        {
            if (input.IndexOf("/") > -1) //ti23:04:12/da07-10-19/YK9010
            {
                string[] siaDataArray = input.Split("/");

                //SIA code and possibly zone should be in the last index
                int codeZoneIdx = siaDataArray.Length - 1;
                output.Code = siaDataArray[codeZoneIdx].Substring(0, 2);
                output.Zone = siaDataArray[codeZoneIdx].Substring(2);

                string time = DateTime.Now.ToShortTimeString();
                string date = DateTime.Now.ToShortDateString();

                foreach (string idx in siaDataArray)
                {
                    switch (idx.Substring(0, 2))
                    {
                        case "ri":
                            output.Area = idx.Substring(2);
                            break;
                        case "id":
                            output.User = idx.Substring(2);
                            break;
                        case "ti":
                            time = idx.Substring(2);
                            break;
                        case "da":
                            date = idx.Substring(2);
                            break;
                        case "va":
                        case "pi":
                            output.Zone = idx.Substring(2);
                            break;
                    }
                }

                output.Timestamp = string.Concat(date, " ", time);
            }
            else //FA8001 //ri1CL008
            {
                if (input.Substring(0, 2) == "ri")
                {
                    output.Area = input.Substring(2, 1);
                    output.Code = input.Substring(3, 2);
                    output.Zone = input.Substring(5);
                }
                else
                {
                    output.Code = input.Substring(0, 2);
                    output.Zone = input.Substring(2);
                }
            }
        }
    }
}
