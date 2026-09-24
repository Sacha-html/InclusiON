using FluentAssertions;
using InclusiON.Application.Mappers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Family;
using Xunit;

namespace InclusiON.Tests.Unit.Mappers
{
    public class FamilyMapperTests
    {
        private static FamilyPersonSummaryResponse Summary() => new();

        private static ActivityResponse Response(decimal? success, int order, int assignmentId = 1, int roadmapOrder = 0) => new()
        {
            CompletedAt = new DateTime(2025, 6, 1, 10, order, 0, DateTimeKind.Utc),
            SuccessPercentage = success,
            AssignmentId = assignmentId,
            Assignment = new ActivityAssignment
            {
                Id = assignmentId,
                ActivityId = assignmentId,
                Activity = new Activity { Id = assignmentId, Title = "Activity", RoadmapOrder = roadmapOrder == 0 ? null : roadmapOrder }
            }
        };

        [Fact]
        public void ApplyProgress_FourConsecutiveFailures_SetsAlert()
        {
            var responses = Enumerable.Range(1, 4).Select(i => Response(30, i));
            var summary = Summary();

            FamilyMapper.ApplyProgress(summary, [], responses);

            summary.HasFrustrationAlert.Should().BeTrue();
        }

        [Fact]
        public void ApplyProgress_FewerFailures_DoesNotSetAlert()
        {
            var responses = Enumerable.Range(1, 3).Select(i => Response(30, i));
            var summary = Summary();

            FamilyMapper.ApplyProgress(summary, [], responses);

            summary.HasFrustrationAlert.Should().BeFalse();
        }

        [Fact]
        public void ApplyProgress_NonConsecutiveFailures_DoesNotSetAlert()
        {
            var responses = new[] { Response(30, 1), Response(30, 2), Response(70, 3), Response(30, 4), Response(30, 5) };
            var summary = Summary();

            FamilyMapper.ApplyProgress(summary, [], responses);

            summary.HasFrustrationAlert.Should().BeFalse();
        }

        [Fact]
        public void ApplyProgress_UnknownSuccessDoesNotCountAsFailure()
        {
            var responses = new[] { Response(30, 1), Response(30, 2), Response(30, 3), Response(null, 4) };
            var summary = Summary();

            FamilyMapper.ApplyProgress(summary, [], responses);

            summary.HasFrustrationAlert.Should().BeFalse();
        }

        [Fact]
        public void ApplyProgress_NegativeGasWithoutFailures_DoesNotSetAlert()
        {
            var sessions = new[] { new ActivitySession { GasScore = -2, Activity = new Activity() } };
            var summary = Summary();

            FamilyMapper.ApplyProgress(summary, sessions, []);

            summary.HasFrustrationAlert.Should().BeFalse();
        }

        [Fact]
        public void ApplyProgress_FailuresSplitAcrossAssignments_DoNotSetAlert()
        {
            var responses = Enumerable.Range(1, 4).Select(i => Response(30, i, assignmentId: i <= 2 ? 1 : 2));
            var summary = Summary();

            FamilyMapper.ApplyProgress(summary, [], responses);

            summary.HasFrustrationAlert.Should().BeFalse();
        }

        [Fact]
        public void ApplyProgress_StartedRoadmapResponseCountsAsCurrentLevel()
        {
            var started = Response(70, 1, roadmapOrder: 4);
            started.CompletedAt = null;
            var summary = Summary();

            FamilyMapper.ApplyProgress(summary, [], [], [started]);

            summary.CurrentRoadmapLevel.Should().Be(4);
            summary.CurrentRoadmapLevelName.Should().Be("Activity");
        }
    }
}
