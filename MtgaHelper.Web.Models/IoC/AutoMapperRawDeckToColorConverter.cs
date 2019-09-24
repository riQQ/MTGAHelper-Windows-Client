﻿using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Lib.Cache;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Web.UI.Shared
{
    public class AutoMapperRawDeckToColorConverter : IValueConverter<ConfigModelRawDeck, string>
    {
        Dictionary<int, Card> dictAllCards;
        UtilColors utilColors;

        public AutoMapperRawDeckToColorConverter(CacheSingleton<ICollection<Card>> cacheCards, UtilColors utilColors)
        {
            this.dictAllCards = cacheCards.Get().ToDictionary(i => i.grpId, i => i);
            this.utilColors = utilColors;
        }

        public string Convert(ConfigModelRawDeck sourceMember, ResolutionContext context)
        {
            if (sourceMember?.CardsMain == null)
                return "";

            try
            {
                var cards = sourceMember.CardsMain.Keys//.Union(sourceMember.CardsSideboard.Keys)
                    .Where(i => dictAllCards.ContainsKey(i))
                    .Select(i => new DeckCard(new CardWithAmount(dictAllCards[i]), false))
                    .ToArray();

                var deck = new Deck(sourceMember.Name, null, cards);
                return utilColors.FromDeck(deck);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"ERROR: whats null? <{sourceMember}> <{sourceMember?.CardsMain}> <{sourceMember?.CardsMain?.Keys}>");
                return "";
            }
        }
    }
}
