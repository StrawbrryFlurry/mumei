namespace Mumei.DependencyInjection.Core;

public abstract class InjectorBloomFilter {
  private const int BucketCount = 4;
  private const int BitsPerBucket = sizeof(ulong) * 8;

  private readonly ulong[] _buckets = new ulong[BucketCount];

  protected void Merge(InjectorBloomFilter bloomFilter) {
    _buckets[0] |= bloomFilter._buckets[0];
    _buckets[1] |= bloomFilter._buckets[1];
    _buckets[2] |= bloomFilter._buckets[2];
    _buckets[3] |= bloomFilter._buckets[3];
  }

  protected void Add(object token) {
    var hashCode = token.GetHashCode();

    var bucketIndex = GetBucketIndex(hashCode);
    var bucketMask = GetBucketMask(hashCode);

    _buckets[bucketIndex] |= bucketMask;
  }

  public bool Contains(object token) {
    var hashCode = token.GetHashCode();

    var bucketIndex = GetBucketIndex(hashCode);
    var bucketMask = GetBucketMask(hashCode);

    return (_buckets[bucketIndex] & bucketMask) == bucketMask;
  }

  private static int GetBucketIndex(int hash) {
    return hash % BucketCount;
  }

  private static ulong GetBucketMask(int hash) {
    return 1UL << (hash % BitsPerBucket);
  }
}