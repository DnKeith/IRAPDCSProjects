using IRAPDCS; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            IRAPHME iRAPDCS = new IRAPHME();
            //var res = iRAPDCS.StartDCSInvoking(60003, 2323);
            ////var res2 = iRAPDCS.WriteMongodbLog("122333", "123", "[{\"ExCode\":60003,\"T133LeafID\":1,\"CommunitID\":2}]");
            //Console.WriteLine(res.ToString());
            ////Console.WriteLine(iRAPDCS.SaveFactMongodbLog(60003,1, 0, 0, 0, 0, "", "", "", ""));
            //Console.ReadKey();

            var res = iRAPDCS.GetOPCStatus("ExCode",60006, "Anonymous", 1, 1319285, 103706, 104269
                , "{\"Equipment_Running_Mode\":true,\"Equipment_Power_On\":true,\"Equipment_Fail\":true,\"Tool_Fail\":true,\"Cycle_Started\":true,\"Equipment_Starvation\":true}");
            //var res2 = iRAPDCS.WriteMongodbLog("122333", "123", "[{\"ExCode\":60003,\"T133LeafID\":1,\"CommunitID\":2}]");
            Console.WriteLine(res.ToString());
            //Console.WriteLine(iRAPDCS.SaveFactMongodbLog(60003, 1, 0, 0, 0, 0, "", "", "", ""));
            Console.ReadKey();
             res = iRAPDCS.GetOPCStatus("ExCode", 60006, "Anonymous", 1, 1319285, 103706, 104269
           , "{\"Equipment_Running_Mode\":true,\"Equipment_Power_On\":true,\"Equipment_Fail\":true,\"Tool_Fail\":true,\"Cycle_Started\":true,\"Equipment_Starvation\":true}");
            Console.WriteLine(res.ToString()); 
            Console.ReadKey();

            res = iRAPDCS.GetOPCStatus("ExCode", 60006, "Anonymous", 1, 1319285, 103706, 104269
          , "{\"Equipment_Running_Mode\":true,\"Equipment_Power_On\":true,\"Equipment_Fail\":true,\"Tool_Fail\":true,\"Cycle_Started\":true,\"Equipment_Starvation\":true}");
            Console.WriteLine(res.ToString());
            Console.ReadKey();
        }
    }
}
