﻿using System;
using System.Threading.Tasks;
using Community.DataObjects;
using Community.DataStore.Abstractions;

using Community.DataStore.Mock;

namespace Community.DataStore.Mock
{
    public class FavoriteStore : BaseStore<Favorite>, IFavoriteStore
    {
        public Task<bool> IsFavorite(string sessionId)
        {
            return Task.FromResult(Settings.IsFavorite(sessionId));
        }

        public override Task<bool> InsertAsync(Favorite item)
        {
            Settings.SetFavorite(item.SessionId, true);
            return Task.FromResult(true);
        }

        public override Task<bool> RemoveAsync(Favorite item)
        {
            Settings.SetFavorite(item.SessionId, false);
            return Task.FromResult(true);
        }

        public async Task DropFavorites()
        {
            await Settings.ClearFavorites();
        }
    }
}

