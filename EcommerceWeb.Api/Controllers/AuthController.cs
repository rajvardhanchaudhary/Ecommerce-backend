using EcommerceWeb.Api.Data;
using EcommerceWeb.Api.Model.DTO;
using EcommerceWeb.Api.Repositories.Interface;
using EcommerceWeb.Api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NZWalks.API.Models.DTO;
using System.Security.Claims;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ITokenRepository tokenRepository;
        private readonly ApplicationDbContext dbContext;
        private readonly IUserRepository userRepository;
        private readonly IEmailSender emailSender;

        public AuthController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenRepository tokenRepository,
            ApplicationDbContext dbContext,
            IUserRepository userRepository,
            IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.tokenRepository = tokenRepository;
            this.dbContext = dbContext;
            this.userRepository = userRepository;
            this.emailSender = emailSender;
        }



        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid input." });

            var existingUser = await userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return Conflict(new ApiResponse { Success = false, Message = "Email already taken." });

            using var transaction = await dbContext.Database.BeginTransactionAsync();

            IdentityUser? user = null;

            try
            {
                user = new IdentityUser
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                };

                var result = await userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(new ApiResponse { Success = false, Message = string.Join("; ", result.Errors.Select(e => e.Description)) });
                }

                // Validate roles and assign
                if (dto.Roles != null && dto.Roles.Any())
                {
                    foreach (var role in dto.Roles)
                    {
                        if (!await RoleExistsAsync(role))
                        {
                            return BadRequest(new ApiResponse { Success = false, Message = $"Role '{role}' does not exist." });
                        }
                    }

                    var roleResult = await userManager.AddToRolesAsync(user, dto.Roles.Distinct());
                    if (!roleResult.Succeeded)
                    {
                        var roleErrors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                        return BadRequest(new ApiResponse { Success = false, Message = roleErrors });
                    }
                }

                // Generate email confirmation token and send email
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, token }, Request.Scheme);

                await emailSender.SendEmailAsync(user.Email, "Confirm your email",
$@"
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background: #f9f9fc; font-family: 'Segoe UI', sans-serif; padding: 0; margin:0"">
  <tr>
    <td align=""center"">
      <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background: #ffffff; border-radius: 8px; overflow: hidden; margin-top: 30px; ;margin-bottom:20px"">
        
        <!-- Header Banner -->
        <tr>
          <td align=""center"" style=""background-color: #7289da; padding: 40px 20px;"">
            <h1 style=""margin: 0; color: white; font-size: 28px;"">Welcome to ShopEase!</h1>
          </td>
        </tr>

        <!-- Illustration -->
        <tr>
          <td align=""center"" style=""padding: 30px 20px 10px;"">
            <img src=""https://cdn-icons-png.flaticon.com/512/7373/7373689.png"" alt=""Welcome image"" width=""120"" style=""max-width: 100%; border: 0;"" />
          </td>
        </tr>

        <!-- Greeting -->
        <tr>
          <td style=""padding: 20px 40px 10px; text-align: center; font-size: 18px; color: #333;"">
            Hey {user.UserName},
          </td>
        </tr>

        <!-- Message -->
        <tr>
          <td style=""padding: 0 40px 30px; text-align: center; font-size: 16px; color: #666;"">
            Thanks for registering an account with <strong>ShopEase</strong>! We're thrilled to have you.
            <br/><br/>
            Before we get started, please verify your email address by clicking the button below.
          </td>
        </tr>

        <!-- Verify Button -->
<tr>
  <td align='center' style='padding-bottom: 40px;'>
    <a href='{confirmationLink}' style='background-color: #7289da; color: #ffffff; padding: 14px 30px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: 500;'>
      Verify Email
    </a>
  </td>
</tr>

        <!-- Footer -->
        <tr>
          <td style=""background-color:  #7289da; padding: 30px 40px; text-align: center;"">
            If you did not create an account, no further action is required.<br><br>
            Sent by ShopEase · Ecom City, USA
          </td>
        </tr>

      </table>
    </td>
  </tr>
</table>



            ");

                // Everything succeeded — commit transaction
                await transaction.CommitAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Registration successful. Please confirm your email."
                });
            }
            catch (Exception ex)
            {
                // Rollback transaction
                await transaction.RollbackAsync();

                // If user was created, delete to prevent partial data
                if (user != null)
                {
                    await userManager.DeleteAsync(user);
                }

                return StatusCode(500, new ApiResponse { Success = false, Message = $"Registration failed: {ex.Message}" });
            }
        }


        private async Task<bool> RoleExistsAsync(string roleName)
        {
            return await roleManager.RoleExistsAsync(roleName);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (dto == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Request body cannot be empty." });

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new ApiResponse { Success = false, Message = "Username and password are required." });

            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new ApiResponse { Success = false, Message = "Invalid username or password." });

            // Check if email confirmed
            if (!await userManager.IsEmailConfirmedAsync(user))
                return Unauthorized(new ApiResponse { Success = false, Message = "Email not confirmed. Please check your inbox." });

            var passwordValid = await userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return Unauthorized(new ApiResponse { Success = false, Message = "Invalid username or password." });

            var is2FAEnabled = await userManager.GetTwoFactorEnabledAsync(user);
            if (is2FAEnabled)
            {
                if (string.IsNullOrWhiteSpace(dto.TwoFactorCode))
                    return BadRequest(new ApiResponse { Success = false, Message = "2FA code is required." });

                var isValid2FA = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, dto.TwoFactorCode);
                if (!isValid2FA)
                    return Unauthorized(new ApiResponse { Success = false, Message = "Invalid 2FA code." });
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = tokenRepository.CreateJWTToken(user, roles.ToList());

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Login successful.",
                Data = new { JwtToken = token }
            });
        }

        [HttpGet("My-Profile")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated." });

            var user = await userRepository.GetCurrentUserAsync(userId);
            if (user == null)
                return NotFound(new ApiResponse { Success = false, Message = "User not found." });

            return Ok(new ApiResponse
            {
                Success = true,
                Data = new { user.Id, user.UserName, user.Email, user.PhoneNumber}
            });
        }

        [HttpPost("Enable-2FA")]
        [Authorize]
        public async Task<IActionResult> Enable2FA([FromBody] Enable2FARequestDto request)
        {
            if (request == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Request body cannot be empty." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated." });

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiResponse { Success = false, Message = "User not found." });

            var is2FAEnabled = await userManager.GetTwoFactorEnabledAsync(user);
            if (is2FAEnabled)
                return BadRequest(new ApiResponse { Success = false, Message = "2FA is already enabled for this user." });

            await userManager.ResetAuthenticatorKeyAsync(user);
            var key = await userManager.GetAuthenticatorKeyAsync(user);

            if (string.IsNullOrWhiteSpace(key))
                return StatusCode(500, new ApiResponse { Success = false, Message = "Failed to generate 2FA key." });

            var result = TwoFactorHelper.Generate2FASetup(
                request.Issuer ?? "MyEcommerceApp",
                user.Email,
                key
            );

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "2FA setup key generated.",
                Data = result
            });
        }

        [HttpPost("Verify-2FA")]
        [Authorize]
        public async Task<IActionResult> Verify2FA([FromBody] TwoFactorVerifyDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new ApiResponse { Success = false, Message = "2FA code is required." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated." });

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiResponse { Success = false, Message = "User not found." });

            var isValid = await userManager.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultAuthenticatorProvider,
                dto.Code
            );

            if (!isValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid 2FA code." });

            var enableResult = await userManager.SetTwoFactorEnabledAsync(user, true);
            if (!enableResult.Succeeded)
            {
                var errors = string.Join("; ", enableResult.Errors.Select(e => e.Description));
                return StatusCode(500, new ApiResponse { Success = false, Message = $"Failed to enable 2FA: {errors}" });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Two-Factor Authentication has been enabled successfully."
            });
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiResponse { Success = false, Message = "User not found." });

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse { Success = false, Message = "Email confirmation failed." });

            return Ok(new ApiResponse { Success = true, Message = "Email confirmed successfully." });
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            // Find the user by email
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                // Optionally return success here to avoid email enumeration attacks
                return Ok(new ApiResponse { Success = true, Message = "If an account with this email exists, a reset link has been sent." });
            }

            // Generate the reset token here
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            // Construct the frontend reset URL
            string frontendResetUrl = $"http://localhost:5173/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(user.Email)}";

            // Send email with the reset link
            await emailSender.SendEmailAsync(user.Email, "Reset Your Password", $@"
   <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background: #f9f9fc; font-family: 'Segoe UI', sans-serif; padding: 0; margin:0"">
      <tr>
        <td align=""center"">
          <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background: #ffffff; border-radius: 8px; overflow: hidden; margin-top: 30px; margin-bottom:20px"">
            
            <!-- Header Banner -->
            <tr>
              <td align=""center"" style=""background-color: #7289da; padding: 40px 20px;"">
                <h1 style=""margin: 0; color: white; font-size: 28px;"">Reset Your Password</h1>
              </td>
            </tr>

            <!-- Illustration -->
            <tr>
              <td align=""center"" style=""padding: 30px 20px 10px;"">
                <img src=""https://cdn-icons-png.flaticon.com/512/1828/1828665.png"" alt=""Reset Password"" width=""120"" style=""max-width: 100%; border: 0;"" />
              </td>
            </tr>

            <!-- Greeting -->
            <tr>
              <td style=""padding: 20px 40px 10px; text-align: center; font-size: 18px; color: #333;"">
                Hello {user.UserName},
              </td>
            </tr>

            <!-- Message -->
            <tr>
              <td style=""padding: 0 40px 30px; text-align: center; font-size: 16px; color: #666;"">
                We received a request to reset your password for your <strong>ShopEase</strong> account.
                <br/><br/>
                To reset your password, please click the button below:
              </td>
            </tr>

            <!-- Reset Password Button -->
            <tr>
              <td align='center' style='padding-bottom: 40px;'>
                <a href='{frontendResetUrl}' style='background-color: #7289da; color: #ffffff; padding: 14px 30px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: 500;'>
                  Reset Password
                </a>
              </td>
            </tr>

            <!-- Footer -->
            <tr>
              <td style=""background-color:  #7289da; padding: 30px 40px; text-align: center; color: white;"">
                If you did not request a password reset, no further action is required.<br><br>
                Sent by ShopEase · Ecom City, USA
              </td>
            </tr>

          </table>
        </td>
      </tr>
    </table>
");


            // Return success response
            return Ok(new ApiResponse { Success = true, Message = "If an account with this email exists, a reset link has been sent." });

        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new ApiResponse { Success = false, Message = "Email, token, and new password are required." });

            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid request." });

            var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(new ApiResponse { Success = false, Message = $"Password reset failed: {errors}" });
            }

            return Ok(new ApiResponse { Success = true, Message = "Password has been reset successfully." });
        }


    }
}



//[HttpPut("Update-Profile")]
//[Authorize]
//public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserDto dto)
//{
//    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//    if (userId == null) return Unauthorized("User not authenticated.");

//    var updatedUser = await userRepository.UpdateCurrentUserAsync(userId, dto.UserName, dto.Email);
//    if (updatedUser == null) return NotFound("User not found.");

//    return Ok(new { updatedUser.Id, updatedUser.UserName, updatedUser.Email });
//}


