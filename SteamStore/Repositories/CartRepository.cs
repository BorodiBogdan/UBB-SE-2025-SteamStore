// <copyright file="CartRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using SteamStore.Constants;
using SteamStore.Data;
using SteamStore.Repositories.Interfaces;

public class CartRepository : ICartRepository
{
    private static string approvedStatus = "Approved";
    private readonly IDataLink dataLink;
    private readonly User user;

    public CartRepository(IDataLink dataLink, User user)
    {
        this.dataLink = dataLink;
        this.user = user;
    }

    public List<Game> GetCartGames()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", this.user.UserIdentifier),
        };

        var result = this.dataLink.ExecuteReader(SqlConstants.GET_ALL_CART_GAMES, parameters);
        List<Game> games = new List<Game>();

        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Identifier = (int)row[SqlConstants.GAME_ID_COLUMN],
                    Name = (string)row[SqlConstants.NAME_COLUMN],
                    Description = (string)row[SqlConstants.DESCRIPTION_COLUMN],
                    ImagePath = (string)row[SqlConstants.IMAGE_URL_COLUMN],
                    Price = Convert.ToDecimal(row[SqlConstants.PRICE_COLUMN]),
                    Status = approvedStatus,
                };
                games.Add(game);
            }
        }

        return games;
    }

    public void AddGameToCart(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", this.user.UserIdentifier),
            new SqlParameter("@game_id", game.Identifier),
        };

        try
        {
            this.dataLink.ExecuteNonQuery(SqlConstants.ADD_GAME_TO_CART, parameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public void RemoveGameFromCart(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", this.user.UserIdentifier),
            new SqlParameter("@game_id", game.Identifier),
        };

        try
        {
            this.dataLink.ExecuteNonQuery(SqlConstants.REMOVE_GAME_FROM_CART, parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public float GetUserFunds()
    {
        return this.user.WalletBalance;
    }
}