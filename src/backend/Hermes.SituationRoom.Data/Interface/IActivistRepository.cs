#nullable enable
namespace Hermes.SituationRoom.Data.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hermes.SituationRoom.Shared.BusinessObjects;

public interface IActivistRepository
{
    Task<Guid> AddAsync(ActivistBo activistBo);

    Task<ActivistBo> GetActivistBoAsync(Guid activistUid);

    Task<IReadOnlyList<ActivistBo>> GetAllActivistBosAsync();

    Task<ActivistBo> UpdateAsync(ActivistBo updatedActivist);

    Task DeleteAsync(Guid activistUid);
}
