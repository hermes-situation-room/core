namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.DataTransferObjects;
using Hermes.SituationRoom.Shared.Exceptions;
using Interfaces;

public class JournalistService(IJournalistRepository journalistRepository, IEncryptionService encryptionService, IBreachedPasswordService breachedPasswordService, IMapper mapper) : IJournalistService
{
    public async Task<JournalistDto> GetJournalistAsync(Guid journalistUid)
    {
        var journalist = await journalistRepository.GetJournalistBoAsync(journalistUid);
        var journalistWithoutSensitiveData = journalist with { Password = null, PasswordHash = null, PasswordSalt = null };
        return mapper.Map<JournalistDto>(journalistWithoutSensitiveData);
    }

    public async Task<IReadOnlyList<JournalistDto>> GetJournalistsAsync()
    {
        var journalists = await journalistRepository.GetAllJournalistBosAsync();
        var journalistsWithoutSensitiveData = journalists.Select(journalist => journalist with { Password = null, PasswordHash = null, PasswordSalt = null }).ToList();
        return mapper.Map<IReadOnlyList<JournalistDto>>(journalistsWithoutSensitiveData);
    }

    public async Task<Guid> CreateJournalistAsync(CreateJournalistRequestDto createJournalistDto)
    {
        if (await breachedPasswordService.IsBreachedAsync(createJournalistDto.Password))
            throw new ValidationBusinessException("Password", "This password has appeared in a known data breach. Please choose a different password.");

        var journalistBo = mapper.Map<JournalistBo>(createJournalistDto);
        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(createJournalistDto.Password);
        journalistBo = journalistBo with { PasswordHash = hash, PasswordSalt = salt };
        return await journalistRepository.AddAsync(journalistBo);
    }

    public async Task<JournalistDto> UpdateJournalistAsync(UpdateJournalistRequestDto updateJournalistDto) 
    {
        var journalistBo = mapper.Map<JournalistBo>(updateJournalistDto);
        var journalist = await journalistRepository.UpdateAsync(journalistBo);
        var journalistWithoutSensitiveData = journalist with { Password = null, PasswordHash = null, PasswordSalt = null };
        return mapper.Map<JournalistDto>(journalistWithoutSensitiveData);
    }

    public Task DeleteJournalistAsync(Guid journalistUid) => journalistRepository.DeleteAsync(journalistUid);
}
