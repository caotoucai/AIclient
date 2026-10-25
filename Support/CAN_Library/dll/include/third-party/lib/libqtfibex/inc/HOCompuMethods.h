#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "HOCompuMethod.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType COMPU-METHODS
 *
 * Container for COMPU-METHOD elements
 */
class ASAM_FIBEX_EXPORT HOCompuMethods
{
public:
    FXFibex *m_fibex = nullptr;

public:
    HOCompuMethods(FXFibex *fibex = nullptr);
    ~HOCompuMethods();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element ho:COMPU-METHOD */
    QList<HOCompuMethod *> m_compuMethods;
};

} // FIBEX
} // ASAM
