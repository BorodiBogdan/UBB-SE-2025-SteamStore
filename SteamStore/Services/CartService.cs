// <copyright file="CartService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamStore.Repositories.Interfaces;
using SteamStore.Services.Interfaces;

public class CartService : ICartService
{
    private ICartRepository cartRepository;

    public CartService(ICartRepository cartRepository)
    {
        this.cartRepository = cartRepository;
    }

    public List<Game> GetCartGames()
    {
        return this.cartRepository.GetCartGames();
    }

    public void RemoveGameFromCart(Game game)
    {
        this.cartRepository.RemoveGameFromCart(game);
    }

    public void AddGameToCart(Game game)
    {
        try
        {
            this.cartRepository.AddGameToCart(game);
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }

    public void RemoveGamesFromCart(List<Game> games)
    {
        foreach (var game in games)
        {
            this.cartRepository.RemoveGameFromCart(game);
        }
    }

    public float GetUserFunds()
    {
        return this.cartRepository.GetUserFunds();
    }

    public decimal GetTotalSumToBePaid()
    {
        decimal total = 0;

        foreach (var game in this.cartRepository.GetCartGames())
        {
            total += (decimal)game.Price;
        }

        return total;
    }
}