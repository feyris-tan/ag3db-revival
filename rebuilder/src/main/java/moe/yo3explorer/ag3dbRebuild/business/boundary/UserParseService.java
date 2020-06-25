package moe.yo3explorer.ag3dbRebuild.business.boundary;

import moe.yo3explorer.ag3dbRebuild.business.control.ByteArrayChannel;
import moe.yo3explorer.ag3dbRebuild.business.entity.ExtractedCharacter;
import org.apache.commons.compress.archivers.sevenz.SevenZArchiveEntry;
import org.apache.commons.compress.archivers.sevenz.SevenZFile;
import org.apache.commons.compress.archivers.tar.TarArchiveEntry;
import org.apache.commons.io.FilenameUtils;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.jetbrains.annotations.Contract;
import org.jetbrains.annotations.NotNull;
import org.tukaani.xz.check.CRC32;

import java.io.IOException;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.stream.Collectors;

public class UserParseService
{
    private String username;
    private HashMap<String, byte[]> map;
    private static Logger logger;
    private static int highestPreview;

    private UserParseService()
    {
        if (logger == null)
        {
            logger = LogManager.getLogger(getClass());
            logger.info("UserParseService reporting in!");
        }
    }

    public static @NotNull UserParseService parseUser(@NotNull TarArchiveEntry tarEntry, byte[] tarBuffer) throws IOException {
        UserParseService result = new UserParseService();
        String[] usernameVines = tarEntry.getName().split("/");
        String filename = usernameVines[usernameVines.length - 1];
        result.username = filename.substring(0,filename.length() - 3);
        boolean goodUser = false;

        ByteArrayChannel channel = new ByteArrayChannel(tarBuffer);
        SevenZFile sevenzip = new SevenZFile(channel,filename);
        SevenZArchiveEntry entry = null;
        result.map = new HashMap<String,byte[]>();
        while ((entry = sevenzip.getNextEntry()) != null)
        {
            if (entry.isDirectory())
                continue;
            if (entry.getName().toLowerCase().endsWith(".js3cmi"))
            {
                if (!goodUser)
                {
                    logger.info(String.format("Found user %s",result.username));
                    goodUser = true;
                }
            }

            int length = (int)entry.getSize();
            byte[] buffer = new byte[length];
            sevenzip.read(buffer,0,length);
            result.map.put(entry.getName(),buffer);
        }
        sevenzip.close();
        return result;
    }

    public List<ExtractedCharacter> extractCharacters()
    {
        List<ExtractedCharacter> result = new LinkedList<>();

        List<String> js3cmiFiles = map.keySet().stream().filter(x -> x.toLowerCase().endsWith(".js3cmi")).collect(Collectors.toList());
        for (String js3cmiFile : js3cmiFiles) {
            logger.debug(String.format("Found character %s",js3cmiFile));
            byte[] buffer = map.get(js3cmiFile);
            String crc32 = getCrc32(buffer);

            ExtractedCharacter extractedCharacter = new ExtractedCharacter();
            extractedCharacter.crc32 = crc32;
            extractedCharacter.js3cmi = buffer;
            extractedCharacter.basename = FilenameUtils.getBaseName(js3cmiFile);

            String sevenZipFilename = String.format("%s/%s.7z",username,extractedCharacter.basename);
            extractedCharacter.sevenzip = map.get(sevenZipFilename);

            String vFilename = String.format("%s/_v - %s.jpg",username,extractedCharacter.basename);
            extractedCharacter.v = map.get(vFilename);

            extractedCharacter.previews = new byte[255][];
            for (int i = 0; i < extractedCharacter.previews.length; i++)
            {
                String previewFilename = String.format("%s/%d - %s.jpg",username,i + 1,extractedCharacter.basename);
                byte[] previewBuffer = map.get(previewFilename);
                if (previewBuffer == null)
                    break;

                extractedCharacter.previews[i] = previewBuffer;
                if (i > highestPreview)
                {
                    highestPreview = i;
                    logger.info(String.format("Highest preview count = %d",highestPreview));
                }
            }
            result.add(extractedCharacter);
        }
        return result;
    }

    private String getCrc32(byte[] buffer)
    {
        CRC32 crc32 = new CRC32();
        crc32.update(buffer,0,buffer.length);
        byte[] crc32Bytes = crc32.finish();
        flip(crc32Bytes);
        String result = bytesToHex(crc32Bytes);
        return result;
    }

    private void flip(byte[] buffer)
    {
        byte temp;
        for (int i = 0; i < buffer.length / 2; i++)
        {
            temp = buffer[i];
            buffer[i] = buffer[buffer.length - i - 1];
            buffer[buffer.length - i - 1] = temp;
        }
    }

    private static final char[] HEX_ARRAY = "0123456789ABCDEF".toCharArray();
    @Contract("_ -> new")
    public static @NotNull String bytesToHex(@NotNull byte[] bytes) {
        char[] hexChars = new char[bytes.length * 2];
        for (int j = 0; j < bytes.length; j++) {
            int v = bytes[j] & 0xFF;
            hexChars[j * 2] = HEX_ARRAY[v >>> 4];
            hexChars[j * 2 + 1] = HEX_ARRAY[v & 0x0F];
        }
        return new String(hexChars);
    }

}
