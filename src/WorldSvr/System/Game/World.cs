using Share.System;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WorldSvr.System.Game
{
    class World
    {
        private readonly ConcurrentDictionary<int, CharUserContext> _characters;

        public World()
        {
            _characters = new ConcurrentDictionary<int, CharUserContext>();
        }

        public bool IsCharacterOnWorld(int characterIdx)
        {
            return _characters.ContainsKey(characterIdx);
        }

        public bool TryEnterWorld(CharUserContext charUserContext)
        {
            return _characters.TryAdd(charUserContext.CharacterIdx, charUserContext);
        }

        public bool TryLeaveWorld(CharUserContext charUserContext)
        {
            return _characters.TryRemove(charUserContext.CharacterIdx, out _);
        }
    }
}
