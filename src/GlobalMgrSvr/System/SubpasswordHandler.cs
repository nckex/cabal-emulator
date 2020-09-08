using System;
using System.Collections.Generic;
using HPT.Common;
using static Share.Protodef;

namespace GlobalMgrSvr.System
{
    class SubpasswordHandler
    {
        public static SubpasswordHandler Instance = Singleton<SubpasswordHandler>.I;

        private readonly Dictionary<int, Dictionary<SubpasswordType, SubpasswordData>> _subpasswordsData;

        public SubpasswordHandler()
        {
            _subpasswordsData = new Dictionary<int, Dictionary<SubpasswordType, SubpasswordData>>();
        }

        public void CreateSubpasswordDataForUserNum(int userNum, SubpasswordType subpasswordType)
        {
            if (!_subpasswordsData.ContainsKey(userNum))
                _subpasswordsData.Add(userNum, new Dictionary<SubpasswordType, SubpasswordData>());

            if (!_subpasswordsData[userNum].ContainsKey(subpasswordType))
                _subpasswordsData[userNum].Add(subpasswordType, new SubpasswordData());
        }

        public void UpdateSubpasswdData(int userNum, SubpasswordType subpasswordType, uint ipAddress32, int hours)
        {
            var expirationTime = (uint)DateTimeOffset.Now.AddHours(hours).ToUnixTimeMilliseconds();

            _subpasswordsData[userNum][subpasswordType].IPAddress32 = ipAddress32;
            _subpasswordsData[userNum][subpasswordType].ExpirationTime = expirationTime;
        }

        public bool IsSubpasswdRequiredFor(int userNum, SubpasswordType subpasswordType, uint ipAddress32)
        {
            if (!_subpasswordsData.ContainsKey(userNum) || !_subpasswordsData[userNum].ContainsKey(subpasswordType))
                return true;

            if (_subpasswordsData[userNum][subpasswordType].IPAddress32 != ipAddress32)
                return true;

            var currentTime = (uint)DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (_subpasswordsData[userNum][subpasswordType].ExpirationTime <= currentTime)
                return true;

            return false;
        }

        public int IncrementAndReturnPassFailCount(int userNum, SubpasswordType subpasswordType)
        {
            _subpasswordsData[userNum][subpasswordType].PassFailCount += 1;
            return _subpasswordsData[userNum][subpasswordType].PassFailCount;
        }

        public int IncrementAndReturnAnswerFailCount(int userNum, SubpasswordType subpasswordType)
        {
            _subpasswordsData[userNum][subpasswordType].AnswerFailCount += 1;
            return _subpasswordsData[userNum][subpasswordType].AnswerFailCount;
        }

        public void ResetPassFailtCount(int userNum, SubpasswordType subpasswordType)
        {
            _subpasswordsData[userNum][subpasswordType].PassFailCount = 0;
        }

        public void ResetAnswerFailCount(int userNum, SubpasswordType subpasswordType)
        {
            _subpasswordsData[userNum][subpasswordType].AnswerFailCount = 0;
        }

        public void RemoveSubpasswdData(int userNum)
        {
            if (_subpasswordsData.ContainsKey(userNum))
                _subpasswordsData.Remove(userNum);
        }

        private class SubpasswordData
        {
            public uint IPAddress32 { get; set; }
            public uint ExpirationTime { get; set; }
            public int PassFailCount { get; set; }
            public int AnswerFailCount { get; set; }
        }
    }
}
