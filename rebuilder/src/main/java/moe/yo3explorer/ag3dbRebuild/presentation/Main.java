package moe.yo3explorer.ag3dbRebuild.presentation;

import moe.yo3explorer.ag3dbRebuild.business.boundary.UserParseService;
import moe.yo3explorer.ag3dbRebuild.business.entity.ExtractedCharacter;
import org.apache.commons.compress.archivers.tar.TarArchiveEntry;
import org.apache.commons.compress.archivers.tar.TarArchiveInputStream;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.jetbrains.annotations.NotNull;

import java.io.*;
import java.util.List;

public class Main
{
    public static void main(String @NotNull [] args) throws IOException {
        Logger logger = LogManager.getLogger(Main.class);
        if (args.length == 0)
        {
            logger.error("Pass the tar file as an argument!");
            return;
        }

        File infile = new File(args[0]);
        if (!infile.isFile())
        {
            logger.error(String.format("%s was not found!",args[0]));
            return;
        }

        FileInputStream level1 = new FileInputStream(infile);
        TarArchiveInputStream level2 = new TarArchiveInputStream(level1);
        TarArchiveEntry tarEntry = null;
        while ((tarEntry = level2.getNextTarEntry()) != null)
        {
            if (tarEntry.isDirectory())
                continue;
            if (!tarEntry.getName().startsWith("chars/"))
                continue;
            if (!tarEntry.getName().toLowerCase().endsWith(".7z"))
                continue;

            int size = (int)tarEntry.getSize();
            byte[] tarBuffer = new byte[size];
            level2.read(tarBuffer,0,size);

            UserParseService foundUser = UserParseService.parseUser(tarEntry, tarBuffer);
            List<ExtractedCharacter> extractedCharacters = foundUser.extractCharacters();
            if (extractedCharacters.size() == 0)
                continue;
        }
    }
}
