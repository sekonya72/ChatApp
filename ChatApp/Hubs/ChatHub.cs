using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ChatApp.DBContext;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Chat.Hubs
{
    public class ChatHub : Hub
    {
        private readonly Func<AppDbContext> _contextFactory;
        private readonly ILogger<ChatHub> _logger;
        private readonly IConfiguration _configuration;

        //  private readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();

        public ChatHub(Func<AppDbContext> contextFactory, ILogger<ChatHub> logger, IConfiguration configuration)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["userId"];

            if (!String.IsNullOrEmpty(userId))
            {
                //_connections.Add(userId, Context.ConnectionId);

                var user = await _contextFactory().Users.FirstOrDefaultAsync(r => r.UserId == userId.ToString());

                if (user == null)
                    await _contextFactory().Users.AddAsync(new User { UserId = userId, ConnectionID = Context.ConnectionId });
                else
                    user.ConnectionID = Context.ConnectionId;

                await _contextFactory().SaveChangesAsync();

            }
            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception e)
        {
            /*var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["userId"];
            if (!String.IsNullOrEmpty(userId))
            {
                _connections.Remove(userId, Context.ConnectionId);
            }*/

            _logger.LogInformation($"OnDisconnectedAsync ------> ConnectionID: {Context?.ConnectionId} ErrorMessage: {e?.Message}");
            await base.OnDisconnectedAsync(e);
        }


        public async Task<string> JoinGroup(UserGroup userGroup)
        {
            var user = await _contextFactory().Users.Include(x => x.UserGroups).FirstOrDefaultAsync(x => x.UserId == userGroup.UserID);

            _contextFactory().TryUpdateManyToMany(
                user.UserGroups,
                new List<UserGroup>() { new UserGroup { UserID = userGroup.UserID, GroupID = userGroup.GroupID } },
                x => x.GroupID);
            try
            {
                await _contextFactory().SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"JoinGroup: {e.Message}");
            }


            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["SecretKey"]));
            var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, userGroup?.UserID)
                    };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
                );

            await Groups.AddToGroupAsync(Context.ConnectionId, userGroup?.GroupID);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public Task LeaveGroup(string groupID)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupID);
        }

        public Task SendMessageToAll(string message)
        {
            return Clients.All.SendAsync("SendMessageToAll", message);
        }

        public Task SendMessageToCaller(string message)
        {
            return Clients.Caller.SendAsync("SendMessageToCaller", message);
        }

    }

    public class ConnectionMapping<T>
    {
        private readonly Dictionary<T, HashSet<string>> _connections =
            new Dictionary<T, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public void Add(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }
    }
}
