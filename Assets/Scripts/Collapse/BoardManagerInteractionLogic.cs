using System.Collections.Generic;
using Collapse.Blocks;
using DG.Tweening;

namespace Collapse {
    /**
     * Partial class for separating the main functions that are needed to be modified in the context of this test
     */
    public partial class BoardManager {
        
        /**
         * Trigger a bomb
         */
        public void TriggerBomb(Bomb bomb)
        {
            bool bombHit = false; //When no other bombs are hit, regenerate. This will only happen with the last bomb in the explosion chain.
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0) { continue; }

                    int x = bomb.GridPosition.x + i;
                    int y = bomb.GridPosition.y + j;
                    if (x >= 0 && x < BoardSize.x && y >= 0 && y < BoardSize.y && blocks[x, y] != null)
                    {
                        bombHit = bombHit || blocks[x, y].Type == BlockType.Bomb;
                        blocks[x, y].Trigger(0.3f); //Note: Magic numbers shouldn't be here
                    }
                }   
            }

            if (!bombHit)
            {
                ScheduleRegenerateBoard();
            }
        }

        /**
         * Trigger a match
         */
        public void TriggerMatch(Block block) {
            // Find all blocks in this match
            var results = new List<Block>();
            var tested = new List<(int row, int col)>();
            FindChainRecursive(block.Type, block.GridPosition.x, block.GridPosition.y, tested, results);
            
            Sequence sequence = DOTween.Sequence();
            // Trigger blocks
            for (var i = 0; i < results.Count; i++)
            {
                int index = i;
                sequence.AppendCallback(() =>
                {
                    results[index].Trigger(0.5f); //Note: Magic numbers shouldn't be here
                });
                
                sequence.AppendInterval(0.2f); //Note: Magic numbers shouldn't be here
            }
            sequence.AppendInterval(0.5f); //Make sure last block finishes it's process

            // Regenerate
            sequence.AppendCallback(ScheduleRegenerateBoard);
        }


        /**
         * Recursively collect all neighbors of same type to build a full list of blocks in this "chain" in the results list
         */
        private void FindChainRecursive(BlockType type, int col, int row, List<(int row, int col)> testedPositions,
            List<Block> results) {

            //Note: this method isn't properly BFS like I wanted because I kept it recursive, I would've implemented a classic BFS approach otherwise
            
            if (results.Count < 1) //First cell
            {
                testedPositions.Add((col, row));
                results.Add(blocks[col, row]);
            }
            
            List<Block> addedBlocks = new List<Block>(); //Used to do recursion only after adding to results ordering them more nicely

            for (int i = 0; i < 4; i++) //Neighbours
            {
                int x = col + (i / 2) * (i % 2 == 0 ? -1 : 1); //results in 0,0,-1,1 offset (probably could be prettier)
                int y = row + (1 - (i / 2)) * (i % 2 == 0 ? -1 : 1); //results in -1,1,0,0 offset (probably could be prettier)

                if ((x >= 0 && x < BoardSize.x && y >= 0 && y < BoardSize.y)
                    && !testedPositions.Contains((x, y)))
                {
                    if (type == blocks[x, y].Type)
                    {
                        addedBlocks.Add(blocks[x, y]);
                        results.Add(blocks[x, y]);
                    }

                    testedPositions.Add((x, y));
                }
            }

            foreach (Block addedBlock in addedBlocks)
            {
                FindChainRecursive(type, addedBlock.GridPosition.x, addedBlock.GridPosition.y, testedPositions, results);
            }
        }
    }
}