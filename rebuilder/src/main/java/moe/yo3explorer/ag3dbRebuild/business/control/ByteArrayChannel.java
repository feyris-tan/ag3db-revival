package moe.yo3explorer.ag3dbRebuild.business.control;

import org.jetbrains.annotations.NotNull;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.channels.SeekableByteChannel;

public class ByteArrayChannel implements SeekableByteChannel {

    public ByteArrayChannel(byte[] buffer)
    {
        this(buffer,0,buffer.length);
    }

    public ByteArrayChannel(byte[] buffer, long offset, long truncatedLength)
    {
        this.backed = buffer;
        this.offset = offset;
        this.truncatedLength = truncatedLength;
    }

    private byte[] backed;
    private long offset;
    private long truncated;
    private final long truncatedLength;
    private boolean closed;

    @Override
    public int read(@NotNull ByteBuffer dst) throws IOException {
        int toRead = dst.limit() - dst.position();
        dst.put(backed,(int)offset,toRead);
        offset += toRead;
        return toRead;
    }

    @Override
    public int write(ByteBuffer src) throws IOException {
        int length = src.limit() - src.position();
        src.get(backed,(int)offset,length);
        offset += length;
        return length;
    }

    @Override
    public long position() throws IOException {
        return offset;
    }

    @Override
    public SeekableByteChannel position(long newPosition) throws IOException {
        offset = newPosition;
        return new ByteArrayChannel(this.backed,this.offset,this.truncatedLength);
    }

    @Override
    public long size() throws IOException {
        return backed.length;
    }

    @Override
    public SeekableByteChannel truncate(long size) throws IOException {
        return new ByteArrayChannel(this.backed,this.offset,size);
    }

    @Override
    public boolean isOpen() {
        return !closed;
    }

    @Override
    public void close() throws IOException {
        closed = true;
    }
}
