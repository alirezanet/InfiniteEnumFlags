using FluentAssertions;
using InfiniteEnumFlags;
using InfiniteEnumFlagsTests.Enums;
using Xunit;

namespace InfiniteEnumFlagsTests;

/// <summary>
/// Verifies that the library's IDs, equality, and name-resolution remain
/// correct when an enum class gains new flag fields over time.
///
/// The core guarantee: IDs encode bit indices, not field order or field count.
/// Adding new flags at new indices never affects any previously stored ID.
/// </summary>
public class FlagEnumEvolutionTests
{
    // ------------------------------------------------------------------ ID stability

    [Fact]
    public void StoredId_IsStillDecodable_AfterEnumGainsNewFlags()
    {
        // Simulate a value stored BEFORE expansion.
        // F1=bit0, F4=bit3 — indices that exist in both the original and expanded enum.
        var beforeExpansion = new Flag<ExpandedTestEnum>(0) | new Flag<ExpandedTestEnum>(3);
        var storedId = beforeExpansion.ToId();

        // Simulate reading that ID back AFTER new flags (F9-F12) have been added.
        var afterExpansion = Flag<ExpandedTestEnum>.FromId(storedId);

        afterExpansion.Should().Be(beforeExpansion);
        afterExpansion.HasFlag(ExpandedTestEnum.F1).Should().BeTrue();
        afterExpansion.HasFlag(ExpandedTestEnum.F4).Should().BeTrue();
        afterExpansion.HasFlag(ExpandedTestEnum.F9).Should().BeFalse();
        afterExpansion.HasFlag(ExpandedTestEnum.F10).Should().BeFalse();
    }

    [Fact]
    public void StoredId_ForNewFlag_DoesNotCollideWithAnyOriginalId()
    {
        // IDs of newly added flags must be distinct from every previously existing ID.
        var originalIds = new HashSet<string>
        {
            ExpandedTestEnum.None.ToId(),
            ExpandedTestEnum.F1.ToId(),
            ExpandedTestEnum.F2.ToId(),
            ExpandedTestEnum.F3.ToId(),
            ExpandedTestEnum.F4.ToId(),
            ExpandedTestEnum.F5.ToId(),
            ExpandedTestEnum.F6.ToId(),
            ExpandedTestEnum.F7.ToId(),
            ExpandedTestEnum.F8.ToId(),
        };

        originalIds.Should().NotContain(ExpandedTestEnum.F9.ToId());
        originalIds.Should().NotContain(ExpandedTestEnum.F10.ToId());
        originalIds.Should().NotContain(ExpandedTestEnum.F11.ToId());
        originalIds.Should().NotContain(ExpandedTestEnum.F12.ToId());
    }

    [Fact]
    public void StoredId_RoundTrips_WhenCombinedWithNewFlags()
    {
        // A combination of an old flag and a new flag encodes and decodes cleanly.
        var mixed = ExpandedTestEnum.F2 | ExpandedTestEnum.F9;
        var id = mixed.ToId();
        var restored = ExpandedTestEnum.FromId(id);

        restored.Should().Be(mixed);
        restored.HasFlag(ExpandedTestEnum.F2).Should().BeTrue();
        restored.HasFlag(ExpandedTestEnum.F9).Should().BeTrue();
        restored.HasFlag(ExpandedTestEnum.F1).Should().BeFalse();
        restored.HasFlag(ExpandedTestEnum.F10).Should().BeFalse();
    }

    // ------------------------------------------------------------------ scoped ID stability

    [Fact]
    public void ScopedId_StoredBeforeExpansion_IsStillDecodable()
    {
        const string scope = "my-app-v1";
        var beforeExpansion = new Flag<ExpandedTestEnum>(0) | new Flag<ExpandedTestEnum>(5);
        var storedScopedId = beforeExpansion.ToScopedId(scope);

        // New flags added to the enum don't touch stored scoped IDs.
        var restored = Flag<ExpandedTestEnum>.FromScopedId(storedScopedId, scope);

        restored.Should().Be(beforeExpansion);
    }

    // ------------------------------------------------------------------ equality stability

    [Fact]
    public void Equality_IsUnaffectedByEnumExpansion()
    {
        // Two flags built with the same indices compare equal regardless of
        // how many other flags exist on the enum class.
        var v1 = ExpandedTestEnum.F3 | ExpandedTestEnum.F7;
        var v2 = new Flag<ExpandedTestEnum>(2) | new Flag<ExpandedTestEnum>(6);

        v1.Should().Be(v2);
        v1.ToId().Should().Be(v2.ToId());
    }

    // ------------------------------------------------------------------ All / AllExcept with new flags

    [Fact]
    public void All_AfterExpansion_IncludesNewFlags()
    {
        // All should now include F9-F12 since they are declared on the expanded enum.
        var all = ExpandedTestEnum.All;

        all.HasFlag(ExpandedTestEnum.F1).Should().BeTrue();
        all.HasFlag(ExpandedTestEnum.F8).Should().BeTrue();
        all.HasFlag(ExpandedTestEnum.F9).Should().BeTrue();
        all.HasFlag(ExpandedTestEnum.F12).Should().BeTrue();
    }

    [Fact]
    public void AllExcept_AfterExpansion_StillExcludesRequestedFlag()
    {
        // AllExcept(F1) should include all new flags AND all original flags except F1.
        var result = ExpandedTestEnum.AllExcept(ExpandedTestEnum.F1);

        result.HasFlag(ExpandedTestEnum.F1).Should().BeFalse();
        result.HasFlag(ExpandedTestEnum.F2).Should().BeTrue();
        result.HasFlag(ExpandedTestEnum.F8).Should().BeTrue();
        result.HasFlag(ExpandedTestEnum.F9).Should().BeTrue();
        result.HasFlag(ExpandedTestEnum.F12).Should().BeTrue();
    }

    [Fact]
    public void AllExcept_OldStoredValue_StillExcludesItAfterExpansion()
    {
        // A value stored before expansion is still correctly excluded by AllExcept.
        var storedValue = ExpandedTestEnum.F3 | ExpandedTestEnum.F5;
        var result = ExpandedTestEnum.AllExcept(storedValue);

        result.HasFlag(ExpandedTestEnum.F3).Should().BeFalse();
        result.HasFlag(ExpandedTestEnum.F5).Should().BeFalse();
        // Old flags not in storedValue are included.
        result.HasFlag(ExpandedTestEnum.F1).Should().BeTrue();
        // New flags (post-expansion) are always included in AllExcept.
        result.HasFlag(ExpandedTestEnum.F9).Should().BeTrue();
    }

    // ------------------------------------------------------------------ GetNames stability

    [Fact]
    public void GetNames_ForStoredValue_ReturnsCorrectNamesAfterExpansion()
    {
        // A value stored before expansion resolves to the same names after expansion.
        var stored = ExpandedTestEnum.F1 | ExpandedTestEnum.F4;
        var names = ExpandedTestEnum.GetNames(stored).ToList();

        names.Should().Contain("F1");
        names.Should().Contain("F4");
        names.Should().NotContain("F9");
        names.Should().NotContain("F10");
    }

    [Fact]
    public void GetNames_AfterExpansion_ReturnsAllDeclaredNames()
    {
        var names = ExpandedTestEnum.GetNames().ToList();

        // Original names preserved.
        names.Should().Contain("None");
        names.Should().Contain("F1");
        names.Should().Contain("F8");
        // New names present.
        names.Should().Contain("F9");
        names.Should().Contain("F12");
    }

    // ------------------------------------------------------------------ what breaks: index reuse

    [Fact]
    public void IndexReuse_WouldBreakStoredIds_DocumentedAsAntiPattern()
    {
        // This test documents the ONE thing that DOES break stored IDs:
        // reassigning an existing index to a different flag.
        //
        // "BadExpansion" would look like:
        //   WAS:  F3 = new(2)
        //   NOW:  F3 = new(5)   <-- index changed!
        //
        // We can't model that with static readonly fields, but we can show
        // that decoding bit 2 as "F5" (index 4) is simply wrong:
        var storedId = new Flag<ExpandedTestEnum>(2).ToId();  // was F3 = new(2)
        var restored = Flag<ExpandedTestEnum>.FromId(storedId);

        // Correctly decodes to bit 2 (F3), NOT bit 4 (F5).
        restored.HasFlag(ExpandedTestEnum.F3).Should().BeTrue();
        restored.HasFlag(ExpandedTestEnum.F5).Should().BeFalse();

        // Rule: NEVER change the index of an existing flag. Only append new flags
        // at indices that have never been used before.
    }
}
