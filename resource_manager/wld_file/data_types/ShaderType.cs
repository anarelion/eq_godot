using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager.wld_file.data_types {
    // Latern Extractor class
    public enum ShaderType {
        Diffuse = 0,
        Transparent25 = 1,
        Transparent50 = 2,
        Transparent75 = 3,
        TransparentAdditive = 4,
        TransparentAdditiveUnlit = 5,
        TransparentMasked = 6,
        DiffuseSkydome = 7,
        TransparentSkydome = 8,
        TransparentAdditiveUnlitSkydome = 9,
        Invisible = 10,
        Boundary = 11,
    }
}
