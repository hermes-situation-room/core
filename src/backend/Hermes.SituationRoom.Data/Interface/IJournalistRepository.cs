#nullable enable
namespace Hermes.SituationRoom.Data.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hermes.SituationRoom.Shared.BusinessObjects;

public interface IJournalistRepository
{
    Task<Guid> AddAsync(JournalistBo journalistBo);

    Task<JournalistBo> GetJournalistBoAsync(Guid journalistUid);

    Task<IReadOnlyList<JournalistBo>> GetAllJournalistBosAsync();

    Task<JournalistBo> UpdateAsync(JournalistBo updatedJournalist);

    Task DeleteAsync(Guid journalistUid);
}
