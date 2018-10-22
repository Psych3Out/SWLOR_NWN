﻿using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;

namespace SWLOR.Game.Server.Service.Contracts
{
    public interface ILanguageService
    {
        string TranslateSnippetForListener(NWPlayer player, SkillType language, string snippet);
        int GetColour(SkillType language);
        string GetName(SkillType language);
        void InitializePlayerLanguages(NWPlayer player);
    }
}