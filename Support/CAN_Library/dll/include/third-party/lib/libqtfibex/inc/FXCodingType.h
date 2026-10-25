#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXRevisedElementType.h"
#include "HOCodedType.h"
#include "HOCompuMethods.h"

#include <QDebug>

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType CODING-TYPE
 *
 * Content model for the entity CODING.
 */
class ASAM_FIBEX_EXPORT CodingType : public FXRevisedElementType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    CodingType(FXFibex *fibex = nullptr);
    ~CodingType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element PHYSICAL-TYPE */
    /** @opt element CODED-TYPE */
    HOCodedType *m_codedType = nullptr;
    /** @opt element COMPU-METHODS */
    HOCompuMethods *m_compuMethods = nullptr;
    /** @todo element MANUFACTURER-EXTENSION */
};

} // FIBEX
} // ASAM
