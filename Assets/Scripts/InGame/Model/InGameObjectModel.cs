using System.Collections.Generic;
using InGame.Object;

namespace InGame.Model
{
    public class InGameObjectModel
    {
        private List<ObjectBase> _objects = new();
        public IReadOnlyList<ObjectBase> Objects => _objects;
        private List<CharacterBase> _characters = new();
        public IReadOnlyList<CharacterBase> Characters => _characters;
        public CharacterBase Player { get; private set; }

        public void AddObject(ObjectBase objectBase) => _objects.Add(objectBase);
        public void RemoveObject(ObjectBase objectBase) => _objects.Remove(objectBase);

        public void AddCharacter(CharacterBase character, bool isPlayer = false)
        {
            _objects.Add(character);
            _characters.Add(character);
            if (isPlayer) Player = Player;
        }

        public void RemoveCharacter(CharacterBase character, bool isPlayer = false)
        {
            _objects.Remove(character);
            _characters.Remove(character);
            if (isPlayer && Player == character) Player = null;
        }
    }
}
