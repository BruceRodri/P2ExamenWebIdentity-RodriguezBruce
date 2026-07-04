using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NorthwindApp.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NorthwindApp.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "Cart";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public CartViewModel GetCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = session.GetString(CartSessionKey);

            if (string.IsNullOrEmpty(cartJson))
            {
                return new CartViewModel();
            }

            try
            {
                var items = JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);
                return new CartViewModel { Items = items ?? new List<CartItemViewModel>() };
            }
            catch
            {
                // Si hay error al deserializar, devolver carrito vacío
                return new CartViewModel();
            }
        }

        public void AddItem(CartItemViewModel item)
        {
            var cart = GetCart();
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);

            if (existingItem != null)
            {
                // Si ya existe, sumamos la cantidad
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                // Si no existe, agregamos nuevo
                cart.Items.Add(item);
            }

            SaveCart(cart);
        }

        public void UpdateQuantity(int productId, short newQuantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                if (newQuantity <= 0)
                {
                    // Si la cantidad es 0 o negativa, eliminamos el producto
                    cart.Items.Remove(item);
                }
                else
                {
                    // Actualizamos la cantidad
                    item.Quantity = newQuantity;
                }
                SaveCart(cart);
            }
        }

        public void RemoveItem(int productId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            _httpContextAccessor.HttpContext.Session.Remove(CartSessionKey);
        }

        private void SaveCart(CartViewModel cart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = JsonConvert.SerializeObject(cart.Items);
            session.SetString(CartSessionKey, cartJson);
        }
    }
}