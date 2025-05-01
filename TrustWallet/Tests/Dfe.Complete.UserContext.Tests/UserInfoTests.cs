using Dfe.Complete.UserContext;
using FluentAssertions;

namespace Dfe.Complete.API.Contracts.Tests.Context
{
	public class UserInfoTests
	{
		[Fact]
		public void ToHeadersKVP_When_NameSet_Returns_Name_Header()
		{
			var sut = new UserInfo();
			sut.Name = Guid.NewGuid().ToString();
			sut.Roles = new[]
			{
				Claims.CaseWorkerRoleClaim,
				Claims.TeamLeaderRoleClaim,
				Claims.AdminRoleClaim,
				"x-filtered-out-role"
			};

			var results = sut.ToHeadersKVP();
			var nameResult = results.FirstOrDefault(x => string.Equals(x.Key, "x-user-context-name", StringComparison.InvariantCultureIgnoreCase));
			nameResult.Value.Should().Be(sut.Name);
		}

		[Fact]
		public void ToHeadersKVP_When_Name_And_Roles_Set_Returns_Role_Headers()
		{
			var sut = new UserInfo();
			sut.Name = Guid.NewGuid().ToString();
			sut.Roles = new[]
			{
				Claims.CaseWorkerRoleClaim,
				Claims.TeamLeaderRoleClaim,
				Claims.AdminRoleClaim
			};

			var results = sut.ToHeadersKVP();
			var roleResults = results.Where(x => x.Key.StartsWith("x-user-context-role-", StringComparison.InvariantCultureIgnoreCase)).ToArray();
			roleResults.Length.Should().Be(3);

			roleResults[0].Key.Should().Be("x-user-context-role-0");
			roleResults[0].Value.Should().Be(Claims.CaseWorkerRoleClaim);
			roleResults[1].Key.Should().Be("x-user-context-role-1");
			roleResults[1].Value.Should().Be(Claims.TeamLeaderRoleClaim);
			roleResults[2].Key.Should().Be("x-user-context-role-2");
			roleResults[2].Value.Should().Be(Claims.AdminRoleClaim);
		}

		[Fact(Skip="Until we implement a prefix on our roles")]
		public void ParseRoleClaims_When_Claims_Filters_To_Only_Concerns_Roles()
		{
			var sut = new UserInfo();
			sut.Name = Guid.NewGuid().ToString();
			var claims = new[]
			{
				Claims.CaseWorkerRoleClaim,
				Claims.TeamLeaderRoleClaim,
				Claims.AdminRoleClaim,
				"x-filtered-out-role"
			};

			var results = UserInfo.ParseRoleClaims(claims);
			results.Length.Should().Be(3);

			results[0].Should().Be(Claims.CaseWorkerRoleClaim);
			results[1].Should().Be(Claims.TeamLeaderRoleClaim);
			results[2].Should().Be(Claims.AdminRoleClaim);
		}

		[Theory]
		[InlineData("x-user-context-name", "x-user-context-role-0", "x-user-context-role-1", "x-user-context-role-2")]
		[InlineData("X-USER-CONTEXT-NAME", "X-USER-CONTEXT-ROLE-0", "X-USER-CONTEXT-ROLE-1", "X-USER-CONTEXT-ROLE-2")]
		[InlineData("X-user-CONTEXT-NAME", "X-user-CONTEXT-ROLE-0", "X-user-CONTEXT-ROLE-1", "X-user-CONTEXT-ROLE-2")]
		public void FromHeaders_Is_Case_Insensitive_And_Returns_Populated_UserInfo(string nameHeader, string roleHeader1, string roleHeader2, string roleHeader3)
		{
			var inputs = new KeyValuePair<string,string>[]
			{
				new(nameHeader ,"John"),
				new(roleHeader1, Claims.CaseWorkerRoleClaim),
				new(roleHeader2, Claims.TeamLeaderRoleClaim),
				new(roleHeader3, Claims.AdminRoleClaim)
			};

			var sut = UserInfo.FromHeaders(inputs);

			sut.Name.Should().Be("John");
			sut.Roles.Should().Contain(Claims.CaseWorkerRoleClaim);
			sut.Roles.Should().Contain(Claims.TeamLeaderRoleClaim);
			sut.Roles.Should().Contain(Claims.AdminRoleClaim);
		}

		[Theory]
		[InlineData("x-user-context-role-0", "x-user-context-role-1", "x-user-context-role-2")]
		[InlineData("X-USER-CONTEXT-ROLE-0", "X-USER-CONTEXT-ROLE-1", "X-USER-CONTEXT-ROLE-2")]
		[InlineData("X-user-CONTEXT-ROLE-0", "X-user-CONTEXT-ROLE-1", "X-user-CONTEXT-ROLE-2")]
		public void FromHeaders_Without_Name_Returns_Null(string roleHeader1, string roleHeader2, string roleHeader3)
		{
			var inputs = new KeyValuePair<string,string>[]
			{
				new("invalid-header" ,"John"),
				new(roleHeader1, Claims.CaseWorkerRoleClaim),
				new(roleHeader2, Claims.TeamLeaderRoleClaim),
				new(roleHeader3, Claims.AdminRoleClaim)
			};

			var sut = UserInfo.FromHeaders(inputs);
			sut.Should().BeNull();
		}

		[Theory]
		[InlineData("x-user-context-name")]
		[InlineData("X-USER-CONTEXT-NAME")]
		[InlineData("X-user-CONTEXT-NAME")]
		public void FromHeaders_Without_Roles_Returns_Null(string nameHeader)
		{
			var inputs = new KeyValuePair<string,string>[]
			{
				new(nameHeader ,"John"),
				new("x-invalid-header-role-0", Claims.CaseWorkerRoleClaim),
			};


			var sut = UserInfo.FromHeaders(inputs);
			sut.Should().BeNull();
		}
	}
}
