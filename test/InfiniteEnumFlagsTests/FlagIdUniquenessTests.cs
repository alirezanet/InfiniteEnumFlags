using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using InfiniteEnumFlags;
using InfiniteEnumFlagsTests.Enums;
using Xunit;

namespace InfiniteEnumFlagsTests;

public class FlagIdUniquenessTests
{
    // ------------------------------------------------------------------ uniqueness

    [Fact]
    public void ToId_AllSingleBitFlags_UpToWideRange_ShouldBeUnique()
    {
        // Every single-bit flag from index 0..2047 (plus the empty flag) must
        // produce a distinct ID. This covers the dense/sparse boundary in both
        // directions many times over.
        var ids = new HashSet<string>();
        ids.Add(new Flag(-1).ToId()).Should().BeTrue();

        for (var i = 0; i < 2048; i++)
        {
            var id = new Flag(i).ToId();
            ids.Add(id).Should().BeTrue($"Flag({i}) produced a duplicate id '{id}'");
        }

        ids.Should().HaveCount(2049);
    }

    [Fact]
    public void ToId_AllPairsOfBits_InSmallRange_ShouldBeUnique()
    {
        // Combinatorial pairs (bit a + bit b) must all be distinct from each
        // other, from the empty flag, and from any single-bit flag.
        var ids = new HashSet<string>();
        ids.Add(new Flag(-1).ToId()).Should().BeTrue();

        for (var i = 0; i < 64; i++)
            ids.Add(new Flag(i).ToId()).Should().BeTrue();

        for (var a = 0; a < 64; a++)
        {
            for (var b = a + 1; b < 64; b++)
            {
                var flag = new Flag(a) | new Flag(b);
                var id = flag.ToId();
                ids.Add(id).Should().BeTrue($"({a},{b}) produced a duplicate id '{id}'");
            }
        }

        // 1 (none) + 64 (singles) + C(64,2) = 1 + 64 + 2016
        ids.Should().HaveCount(2081);
    }

    [Fact]
    public void ToId_AcrossDenseSparseBoundary_ShouldBeUnique()
    {
        // Construct flags that should each pick a different encoding branch.
        var samples = new[]
        {
            new Flag(-1),                                  // empty
            new Flag(0),
            new Flag(63),
            new Flag(64),
            new Flag(0)  | new Flag(1),
            new Flag(0)  | new Flag(63),
            new Flag(0)  | new Flag(64),
            new Flag(7)  | new Flag(8) | new Flag(9),
            new Flag(100),                                  // sparse wins
            new Flag(101),                                  // sparse wins
            new Flag(100) | new Flag(101),                  // sparse wins
            new Flag(1000),                                 // sparse wins big
            new Flag(10_000),                               // sparse wins very big
            new Flag(0) | new Flag(10_000),                 // mixed
        };

        var ids = samples.Select(f => f.ToId()).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ToId_RandomizedHighIndexFlags_ShouldBeUnique()
    {
        // Stress: 5000 random flags drawn from a wide index space with varying
        // popcounts. No collisions are tolerated.
        const int iterations = 5000;
        var rng = new System.Random(12345);
        var ids = new HashSet<string>(iterations);
        var seenValues = new HashSet<Flag<object>>(iterations);

        for (var i = 0; i < iterations; i++)
        {
            var bitCount = rng.Next(1, 12);
            var flag = new Flag<object>(-1);
            for (var b = 0; b < bitCount; b++)
                flag |= new Flag<object>(rng.Next(0, 20_000));

            var id = flag.ToId();
            if (seenValues.Add(flag))
                ids.Add(id).Should().BeTrue($"Distinct flag value collided on id '{id}'");
            else
                ids.Should().Contain(id); // already seen flag must reuse its id
        }
    }

    // ------------------------------------------------------------------ canonicalization

    [Fact]
    public void ToId_EqualFlagsBuiltDifferently_ShouldProduceIdenticalId()
    {
        // Same logical bits, very different construction lengths => same id.
        var a = new Flag(5, 10);
        var b = new Flag(5, 50);
        var c = new Flag(5, 10_000);

        a.Should().Be(b);
        b.Should().Be(c);

        a.ToId().Should().Be(b.ToId());
        b.ToId().Should().Be(c.ToId());
    }

    [Fact]
    public void ToId_OperatorChainsThatProduceSameValue_ShouldProduceSameId()
    {
        // Build the same value via different operator paths.
        var v1 = new Flag(0) | new Flag(1) | new Flag(2);
        var v2 = (new Flag(0) | new Flag(2)) | new Flag(1);
        var v3 = (new Flag(0) | new Flag(1) | new Flag(2) | new Flag(3)) ^ new Flag(3);

        v1.Should().Be(v2);
        v2.Should().Be(v3);

        var id = v1.ToId();
        v2.ToId().Should().Be(id);
        v3.ToId().Should().Be(id);
    }

    // ------------------------------------------------------------------ round-trip

    [Fact]
    public void ToId_FromId_RoundTrip_AcrossWideIndexRange_ShouldBeLossless()
    {
        for (var i = -1; i < 1024; i++)
        {
            var original = new Flag(i);
            var roundTripped = Flag.FromId(original.ToId());
            roundTripped.Should().Be(original);
        }
    }

    [Fact]
    public void ToId_FromId_RoundTrip_RandomizedFlags_ShouldBeLossless()
    {
        var rng = new System.Random(67890);
        for (var n = 0; n < 1000; n++)
        {
            var bitCount = rng.Next(0, 20);
            var flag = new Flag<object>(-1);
            for (var b = 0; b < bitCount; b++)
                flag |= new Flag<object>(rng.Next(0, 50_000));

            var id = flag.ToId();
            Flag<object>.FromId(id).Should().Be(flag);
        }
    }

    [Fact]
    public void ToId_FromId_StableAcrossReencoding_ShouldBeIdempotent()
    {
        // FromId(ToId(x)) must round-trip, AND ToId(FromId(s)) must equal s
        // for any s produced by ToId. This proves the encoder is canonical
        // (no two ids decode to the same value, no value encodes two ways).
        var samples = new[]
        {
            new Flag(-1),
            new Flag(0),
            new Flag(63) | new Flag(64),
            new Flag(7) | new Flag(8) | new Flag(9) | new Flag(150),
            new Flag(10_000),
        };

        foreach (var flag in samples)
        {
            var id1 = flag.ToId();
            var decoded = Flag.FromId(id1);
            var id2 = decoded.ToId();

            decoded.Should().Be(flag);
            id2.Should().Be(id1);
        }
    }

    // ------------------------------------------------------------------ scoped ids

    [Fact]
    public void ToScopedId_DifferentScopes_ShouldProduceDifferentIds()
    {
        var flag = TestEnum.F1 | TestEnum.F4;

        var idA = flag.ToScopedId("scope-a");
        var idB = flag.ToScopedId("scope-b");
        var idC = flag.ToScopedId("scope-c");

        new[] { idA, idB, idC }.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ToScopedId_DifferentValuesInSameScope_ShouldBeUnique()
    {
        const string scope = "uniqueness-test";
        var ids = new HashSet<string>();

        for (var i = 0; i < 256; i++)
            ids.Add(new Flag<TestEnum>(i).ToScopedId(scope))
               .Should().BeTrue($"Flag({i}) produced a duplicate scoped id");

        ids.Should().HaveCount(256);
    }

    [Fact]
    public void ToScopedId_FromScopedId_RoundTrip_ShouldBeLossless()
    {
        const string scope = "permissions-v1";
        var rng = new System.Random(2024);

        for (var n = 0; n < 200; n++)
        {
            var bitCount = rng.Next(0, 10);
            var flag = new Flag<TestEnum>(-1);
            for (var b = 0; b < bitCount; b++)
                flag |= new Flag<TestEnum>(rng.Next(0, 5_000));

            var id = flag.ToScopedId(scope);
            Flag<TestEnum>.FromScopedId(id, scope).Should().Be(flag);
        }
    }
}
