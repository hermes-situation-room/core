#nullable enable
namespace Hermes.SituationRoom.Data.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hermes.SituationRoom.Shared.BusinessObjects;

public interface IPrivacyLevelPersonalRepository
{
    Task<Guid> AddAsync(PrivacyLevelPersonalBo privacyLevelPersonalBo);

    Task<PrivacyLevelPersonalBo> GetPrivacyLevelPersonalBoAsync(Guid uid);

    Task<PrivacyLevelPersonalBo> GetPrivacyLevelPersonalBoAsync(Guid ownerUid, Guid consumerUid);

    Task<PrivacyLevelPersonalBo> UpdateAsync(PrivacyLevelPersonalBo updatedPersonalLevelPersonal);

    Task DeleteAsync(Guid privacyLevelPersonalUid);
}
