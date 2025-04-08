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
        return this.cartRepository.getCartGames();
    }

    public void RemoveGameFromCart(Game game)
    {
        this.cartRepository.removeGameFromCart(game);
    }

    public void AddGameToCart(Game game)
    {
        try
        {
            this.cartRepository.addGameToCart(game);
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
            this.cartRepository.removeGameFromCart(game);
        }
    }

    public float GetUserFunds()
    {
        return this.cartRepository.getUserFunds();
    }

    public decimal GetTotalSumToBePaid()
    {
        return this.cartRepository.getCartGames().Sum(game => (decimal)game.Price);
    }
}