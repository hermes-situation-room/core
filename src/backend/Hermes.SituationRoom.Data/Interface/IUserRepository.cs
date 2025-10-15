﻿#nullable enable
namespace Hermes.SituationRoom.Data.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hermes.SituationRoom.Shared.BusinessObjects;

public interface IUserRepository
{
    Task<Guid> AddAsync(UserBo userBo);

    Task<UserBo> GetUserBoAsync(Guid userUid);

    Task<Guid?> FindJournalistIdByEmailAsync(string emailAddress);

    Task<UserBo> GetUserBoByEmailAsync(string emailAddress);

    Task<UserProfileBo> GetUserProfileBoAsync(Guid userUid, Guid consumerUid);

    Task<string> GetDisplayNameAsync(Guid userUid);

    Task<IReadOnlyList<UserBo>> GetAllUserBosAsync();

    Task<UserBo> Update(UserBo updatedUser);

    Task Delete(Guid userUid);
}
