using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;
using Chat.Hubs;
using ChatApp.DBContext;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IHubContext<ChatHub> _hub;
        private readonly Func<AppDbContext> _contextFactory;
        private readonly IConfiguration _configuration;
        public ChatController(Func<AppDbContext> contextFactory, ILogger<ChatController> logger, IHubContext<ChatHub> hub, IConfiguration configuration)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _hub = hub;
            _configuration = configuration;
        }


        [HttpPost("group/invite")]
        [Authorize]
        public async Task InviteUserToGroup(UserGroup userGroup)
        {
            try
            {
                var user = await _contextFactory().Users.FirstOrDefaultAsync(e => e.UserId == userGroup.UserID);

                if (user != null)
                    await _hub.Clients.Client(user.ConnectionID).SendAsync("InviteUserToGroup", userGroup?.GroupID);
            }
            catch (Exception e)
            {
                _logger.LogError($"InviteUser: {e.Message}");
            }
        }

        [HttpPost("group/{id}/send")]
        [Authorize]
        public async Task SendMessageToGroup(string id, Message message)
        {
            try
            {
                await _contextFactory().Messages.AddAsync(message);
                await _contextFactory().SaveChangesAsync();
                await _hub.Clients.Group(id).SendAsync("SendMessageToGroup", message);
            }
            catch (Exception e)
            {
                _logger.LogError($"SendMessageToGroup: {e.Message}");
            }
        }

        [HttpGet("group/{id}/users")]
        [Authorize]
        public Task<List<UserGroup>> GetGroupUsers(string id)
        {
            try
            {
                return  _contextFactory().UserGroups.Where(x => x.GroupID == id).ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"GetGroupUsers: {e.Message}");
                return Task.FromResult(new List<UserGroup>());
            }
        }
    }
}
