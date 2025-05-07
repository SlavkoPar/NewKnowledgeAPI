﻿using NewKnowledgeAPI.Common;
using NewKnowledgeAPI.HistFilter.Model;
using Newtonsoft.Json;

namespace NewKnowledgeAPI.Hist.Model
{

    public class HistoryFilterEx
    {
        public HistoryFilterEx(HistoryFilter? historyFilter, string msg)
        {
            HistoryFilter = historyFilter;
            Msg = msg;
        }

        public HistoryFilter? HistoryFilter { get; set; }
        public string Msg { get; set; }

        internal void Deconstruct(out HistoryFilter? historyFilter, out string msg)
        {
            historyFilter = HistoryFilter;
            msg = Msg;
        }
    }
}
