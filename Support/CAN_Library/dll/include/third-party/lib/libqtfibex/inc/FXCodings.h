#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXCodingType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType CODINGS
 *
 * Top level element of all codings
 */
class ASAM_FIBEX_EXPORT FXCodings
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXCodings(FXFibex *fibex = nullptr);
    ~FXCodings();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element CODING */
    QHash<QString, CodingType*> m_codings;
};

} // FIBEX
} // ASAM
