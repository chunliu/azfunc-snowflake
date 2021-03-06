using System;

namespace chunliu.demo
{
    public interface ISnowflakeIdWorker
    {
        long NextId();
        long WorkerId {get;}
        long DatacenterId {get;}
    }
    // Based on https://blog.twitter.com/engineering/en_us/a/2010/announcing-snowflake.html
    public class SnowflakeIdWorker : ISnowflakeIdWorker
    {
        // Anchor timestamp (2020-09-01)
        private const long twepoch = 1598918400000L;
        // Bits for worker id
        private const int workerIdBits = 5;
        // Bits for data center id
        private const int datacenterIdBits = 5;
        // Max id of worker, 31 (0b11111)
        private long maxWorkerId = -1 ^ (-1 << workerIdBits);
        // Max id of data center, 31 (0b11111)
        private long maxDatacenterId = -1 ^ (-1 << datacenterIdBits);
        // Bits for the sequence number
        private const int sequenceBits = 12;
        // The position of work id
        private int workerIdShift = sequenceBits;
        // The position of data center id
        private int datacenterIdShift = sequenceBits + workerIdBits;
        // The position of the time stamp
        private int timestampLeftShift = sequenceBits + workerIdBits + datacenterIdBits;
        // The mask of the sequence (4095 (0b111111111111=0xfff=4095))
        private int sequenceMask = -1 ^ (-1 << sequenceBits);
        private long workerId;
        private long datacenterId;
        private long sequence = 0;
        private long lastTimestamp = -1;

        public long WorkerId
        {
            get => this.workerId;
        }
        public long DatacenterId
        {
            get => this.datacenterId;
        }

        public SnowflakeIdWorker(int workerId, int datacenterId)
        {
            if (workerId > maxWorkerId || workerId < 0)
            {
                throw new ArgumentException($"Worker Id can't be greater than {maxWorkerId} or less than 0");
            }
            if (datacenterId > maxDatacenterId || datacenterId < 0)
            {
                throw new ArgumentException($"Data center Id can't be greater than {maxDatacenterId} or less than 0");
            }
            this.workerId = workerId;
            this.datacenterId = datacenterId;
        }

        public long NextId()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (timestamp < lastTimestamp)
            {
                throw new Exception($"Clock moved backwards. Refusing to generate id for {lastTimestamp} milliseconds");
            }

            if (lastTimestamp == timestamp)
            {
                // If it's the same time, generate sequence within a millis
                sequence = (sequence + 1) & sequenceMask;
                if (sequence == 0) 
                {
                    // If the sequence is over the max, wait till next millis
                    timestamp = TillNextMillis(lastTimestamp);
                }
            }
            else
            {
                // Reset the sequence for the new time stamp
                sequence = 0;
            }
            lastTimestamp = timestamp;

            return ((timestamp - twepoch) << timestampLeftShift)
                | (datacenterId << datacenterIdShift)
                | (workerId << workerIdShift)
                | sequence;
        }

        private long TillNextMillis(long timestamp)
        {
            long nextTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (nextTimestamp <= timestamp)
            {
                nextTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            return nextTimestamp;
        }
    }
}