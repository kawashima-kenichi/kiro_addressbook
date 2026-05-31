using System.Security.Claims;
using AddressBook.Application.DTOs;
using AddressBook.Application.Interfaces;
using AddressBook.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AddressBook.API.Controllers;

[ApiController]
[Route("api/contacts")]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactsController> _logger;

    public ContactsController(IContactService contactService, ILogger<ContactsController> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    /// <summary>
    /// 連絡先一覧を取得（ページネーション、ソート付き）
    /// </summary>
    /// <param name="page">ページ番号（デフォルト: 1）</param>
    /// <param name="pageSize">1ページあたりの件数（デフォルト: 50）</param>
    /// <param name="sortBy">ソート順（name, name_desc, created_at, created_at_desc, updated_at, updated_at_desc）</param>
    [HttpGet]
    [ProducesResponseType(typeof(ContactListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ContactErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetContacts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? sortBy = null)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var (contacts, totalCount) = await _contactService.GetContactsAsync(userId, page, pageSize, sortBy);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var response = new ContactListResponse
            {
                Contacts = contacts.ToList(),
                Pagination = new PaginationDto
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPrevious = page > 1,
                    HasNext = page < totalPages
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "連絡先一覧の取得中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError, new ContactErrorResponse
            {
                Message = "連絡先を読み込めません。後でもう一度お試しください。"
            });
        }
    }

    /// <summary>
    /// 指定IDの連絡先を取得
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ContactErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetContactById(Guid id)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var contact = await _contactService.GetContactByIdAsync(userId, id);

            if (contact == null)
            {
                return NotFound(new ContactErrorResponse
                {
                    Message = "連絡先が見つかりません"
                });
            }

            return Ok(contact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "連絡先の取得中にエラーが発生しました: {ContactId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ContactErrorResponse
            {
                Message = "連絡先を読み込めません。後でもう一度お試しください。"
            });
        }
    }

    /// <summary>
    /// 新しい連絡先を作成
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContactSuccessResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ContactErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ContactErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateContact([FromBody] CreateContactRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            // Validation is handled by FluentValidation middleware
            if (!ModelState.IsValid)
            {
                return BadRequest(new ContactErrorResponse
                {
                    Message = "入力データが無効です",
                    ValidationErrors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            var contact = await _contactService.CreateContactAsync(userId, request);

            var response = new ContactSuccessResponse
            {
                Success = true,
                Message = $"連絡先「{contact.Name}」が正常に追加されました",
                Contact = contact
            };

            return CreatedAtAction(nameof(GetContactById), new { id = contact.Id }, response);
        }
        catch (DuplicateContactNameException ex)
        {
            return BadRequest(new ContactErrorResponse
            {
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "連絡先の作成中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError, new ContactErrorResponse
            {
                Message = "連絡先を保存できません。もう一度お試しください。"
            });
        }
    }

    /// <summary>
    /// 既存の連絡先を更新
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ContactSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ContactErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ContactErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateContact(Guid id, [FromBody] UpdateContactRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            // Validation is handled by FluentValidation middleware
            if (!ModelState.IsValid)
            {
                return BadRequest(new ContactErrorResponse
                {
                    Message = "入力データが無効です",
                    ValidationErrors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            var contact = await _contactService.UpdateContactAsync(userId, id, request);

            var response = new ContactSuccessResponse
            {
                Success = true,
                Message = $"連絡先「{contact.Name}」が正常に更新されました",
                Contact = contact
            };

            return Ok(response);
        }
        catch (ContactNotFoundException ex)
        {
            return NotFound(new ContactErrorResponse
            {
                Message = ex.Message
            });
        }
        catch (DuplicateContactNameException ex)
        {
            return BadRequest(new ContactErrorResponse
            {
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "連絡先の更新中にエラーが発生しました: {ContactId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ContactErrorResponse
            {
                Message = "連絡先を更新できません。もう一度お試しください。"
            });
        }
    }

    /// <summary>
    /// 連絡先を削除
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ContactSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ContactErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteContact(Guid id)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            // Get contact name before deletion for success message
            var contact = await _contactService.GetContactByIdAsync(userId, id);
            if (contact == null)
            {
                return NotFound(new ContactErrorResponse
                {
                    Message = "連絡先が見つかりません"
                });
            }

            await _contactService.DeleteContactAsync(userId, id);

            var response = new ContactSuccessResponse
            {
                Success = true,
                Message = $"連絡先「{contact.Name}」が正常に削除されました"
            };

            return Ok(response);
        }
        catch (ContactNotFoundException ex)
        {
            return NotFound(new ContactErrorResponse
            {
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "連絡先の削除中にエラーが発生しました: {ContactId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ContactErrorResponse
            {
                Message = "連絡先を削除できません。もう一度お試しください。"
            });
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
