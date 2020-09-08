using HPT.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldSvr.System.Game
{
    class WorldManager
    {
        public static WorldManager Instance = Singleton<WorldManager>.I;

        private readonly Dictionary<int, World> _worlds;

        public WorldManager()
        {
            _worlds = new Dictionary<int, World>();
        }

        public void InitWorldTeste()
        {
            _worlds.Add(1, new World());
            _worlds.Add(2, new World());
            _worlds.Add(3, new World());
        }

        public bool TryGetWorld(int worldIdx, out World world)
        {
            return _worlds.TryGetValue(worldIdx, out world);
        }
    }
}
