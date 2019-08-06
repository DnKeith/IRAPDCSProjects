using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRAPDCS.Entities
{
    /// <summary>
    /// 日志文件
    /// </summary>
    [BsonIgnoreExtraElements]
    public class GraphEntity
    { 
        public Int64 LogID { get; set; }
        public string ExCode { get; set; }
        public int CommunityID { get; set; }
        public string UserCode { get; set; }
        public Int64 SysLogID { get; set; }
        public int T133LeafID { get; set; }
        public int T216LeafID { get; set; }
        public int T102LeafID { get; set; }
        public int T107LeafID { get; set; }
        public string WIP_Code { get; set; }
        public string WIP_ID_Type_Code { get; set; }
        public string WIP_ID_Code { get; set; }
        public string Params { get; set; }
        // public List<EdgeEntity> Rows { get; set; }
        public string StoreTime { get; set; }
    } 
}
